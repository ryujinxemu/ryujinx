﻿using ARMeilleure.Translation;
using NUnit.Framework;
using Ryujinx.Cpu.Jit;
using Ryujinx.Tests.Memory;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Tests.Cpu
{
    internal class EnvironmentTests
    {
        private static Translator _translator;

        private void EnsureTranslator()
        {
            // Create a translator, as one is needed to register the signal handler or emit methods.
            _translator ??= new Translator(new JitMemoryAllocator(), new MockMemoryManager(), true);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private float GetDenormal()
        {
            return BitConverter.Int32BitsToSingle(1);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private float GetZero()
        {
            return BitConverter.Int32BitsToSingle(0);
        }

        /// <summary>
        /// This test ensures that managed methods do not reset floating point control flags.
        /// This is used to avoid changing control flags when running methods that don't require it, such as SVC calls, software memory...
        /// </summary>
        [Test]
        public void FpFlagsPInvoke()
        {
            EnsureTranslator();

            // Subnormal results are not flushed to zero by default.
            // This operation should not be allowed to do constant propagation, hence the methods that explicitly disallow inlining.
            Assert.AreNotEqual(GetDenormal() + GetZero(), 0f);

            bool methodCalled = false;
            bool isFz = false;

            var managedMethod = () =>
            {
                // Floating point math should not modify fp flags.
                float test = 2f * 3.5f;

                if (test < 4f)
                {
                    throw new System.Exception("Sanity check.");
                }

                isFz = GetDenormal() + GetZero() == 0f;

                try
                {
                    if (test >= 4f)
                    {
                        throw new System.Exception("Always throws.");
                    }
                }
                catch
                {
                    // Exception handling should not modify fp flags.

                    methodCalled = true;
                }
            };

            var method = TranslatorTestMethods.GenerateFpFlagsPInvokeTest();

            int result = method(Marshal.GetFunctionPointerForDelegate(managedMethod));

            // Subnormal results are not flushed to zero by default, which we should have returned to exiting the method.
            Assert.AreNotEqual(GetDenormal() + GetZero(), 0f);

            Assert.True(result == 0);
            Assert.True(methodCalled);
            Assert.True(isFz);
        }
    }
}
