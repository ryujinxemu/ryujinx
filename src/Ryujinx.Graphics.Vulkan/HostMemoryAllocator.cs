﻿using Ryujinx.Common;
using Ryujinx.Common.Collections;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ryujinx.Graphics.Vulkan
{
    internal class HostMemoryAllocator
    {
        private struct HostMemoryAllocation
        {
            public readonly Auto<MemoryAllocation> Allocation;
            public readonly IntPtr Pointer;
            public readonly ulong Size;

            public ulong Start => (ulong)Pointer;
            public ulong End => (ulong)Pointer + Size;

            public HostMemoryAllocation(Auto<MemoryAllocation> allocation, IntPtr pointer, ulong size)
            {
                Allocation = allocation;
                Pointer = pointer;
                Size = size;
            }
        }

        private readonly MemoryAllocator _allocator;
        private readonly Vk _api;
        private readonly Device _device;
        private readonly object _lock = new();

        private List<HostMemoryAllocation> _allocations;
        private IntervalTree<ulong, HostMemoryAllocation> _allocationTree;

        public HostMemoryAllocator(MemoryAllocator allocator, Vk api, Device device)
        {
            _allocator = allocator;
            _api = api;
            _device = device;

            _allocations = new List<HostMemoryAllocation>();
            _allocationTree = new IntervalTree<ulong, HostMemoryAllocation>();
        }

        public unsafe bool TryImport(
            MemoryRequirements requirements,
            MemoryPropertyFlags flags,
            IntPtr pointer,
            ulong size)
        {
            lock (_lock)
            {
                // Does a compatible allocation exist in the tree?
                var allocations = new HostMemoryAllocation[10];

                ulong start = (ulong)pointer;
                ulong end = start + size;

                int count = _allocationTree.Get(start, end, ref allocations);

                // A compatible range is one that where the start and end completely cover the requested range.
                for (int i = 0; i < count; i++)
                {
                    HostMemoryAllocation existing = allocations[i];

                    if (start >= existing.Start && end <= existing.End)
                    {
                        try
                        {
                            existing.Allocation.IncrementReferenceCount();

                            Console.WriteLine($"Existing at {start:x16} {end:x8}");

                            return true;
                        }
                        catch (InvalidOperationException)
                        {
                            // Can throw if the allocation has been disposed.
                            // Just continue the search if this happens.
                        }
                    }
                }

                int memoryTypeIndex = _allocator.FindSuitableMemoryTypeIndex(requirements.MemoryTypeBits, flags);
                if (memoryTypeIndex < 0)
                {
                    return default;
                }

                nint pageAlignedPointer = BitUtils.AlignDown(pointer, Environment.SystemPageSize);
                nint pageAlignedEnd = BitUtils.AlignUp((nint)((ulong)pointer + size), Environment.SystemPageSize);
                ulong pageAlignedSize = (ulong)(pageAlignedEnd - pageAlignedPointer);

                ImportMemoryHostPointerInfoEXT importInfo = new ImportMemoryHostPointerInfoEXT()
                {
                    SType = StructureType.ImportMemoryHostPointerInfoExt,
                    HandleType = ExternalMemoryHandleTypeFlags.HostAllocationBitExt,
                    PHostPointer = (void*)pageAlignedPointer
                };

                var memoryAllocateInfo = new MemoryAllocateInfo()
                {
                    SType = StructureType.MemoryAllocateInfo,
                    AllocationSize = pageAlignedSize,
                    MemoryTypeIndex = (uint)memoryTypeIndex,
                    PNext = &importInfo
                };

                Console.WriteLine($"{pageAlignedPointer:x16} {pageAlignedSize:x8}");

                Result result = _api.AllocateMemory(_device, memoryAllocateInfo, null, out var deviceMemory);

                if (result < Result.Success)
                {
                    Console.WriteLine($"failed :(");
                    return false;
                }

                var allocation = new MemoryAllocation(this, deviceMemory, pageAlignedPointer, 0, pageAlignedSize);
                var allocAuto = new Auto<MemoryAllocation>(allocation);
                var hostAlloc = new HostMemoryAllocation(allocAuto, pageAlignedPointer, pageAlignedSize);

                allocAuto.IncrementReferenceCount();
                allocAuto.Dispose(); // Kept alive by ref count only.

                // Register this mapping for future use.

                _allocationTree.Add(hostAlloc.Start, hostAlloc.End, hostAlloc);
                _allocations.Add(hostAlloc);
            }

            return true;
        }

        public (Auto<MemoryAllocation>, ulong) GetExistingAllocation(IntPtr pointer, ulong size)
        {
            lock (_lock)
            {
                // Does a compatible allocation exist in the tree?
                var allocations = new HostMemoryAllocation[10];

                ulong start = (ulong)pointer;
                ulong end = start + size;

                int count = _allocationTree.Get(start, end, ref allocations);

                // A compatible range is one that where the start and end completely cover the requested range.
                for (int i = 0; i < count; i++)
                {
                    HostMemoryAllocation existing = allocations[i];

                    if (start >= existing.Start && end <= existing.End)
                    {
                        return (existing.Allocation, start - existing.Start);
                    }
                }

                throw new InvalidOperationException($"No host allocation was prepared for requested range {pointer:x16}:{size:x16}.");
            }
        }

        public unsafe void Free(DeviceMemory memory, ulong offset, ulong size)
        {
            lock (_lock)
            {
                _allocations.RemoveAll(allocation =>
                {
                    if (allocation.Allocation.GetUnsafe().Memory.Handle == memory.Handle)
                    {
                        _allocationTree.Remove(allocation.Start, allocation);
                        Console.WriteLine($"freed {BitUtils.AlignDown(allocation.Pointer, Environment.SystemPageSize)}");
                        return true;
                    }

                    return false;
                });
            }

            _api.FreeMemory(_device, memory, null);
        }
    }
}
