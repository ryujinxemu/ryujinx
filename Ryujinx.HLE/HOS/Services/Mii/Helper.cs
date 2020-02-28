﻿using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Utilities;
using System;

namespace Ryujinx.HLE.HOS.Services.Mii
{
    class Helper
    {
        public static ushort CalculateCrc16BE(ReadOnlySpan<byte> data, int crc = 0)
        {
            const ushort Poly = 0x1021;

            for (int i = 0; i < data.Length; i++)
            {
                crc ^= data[i] << 8;

                for (int j = 0; j < 8; j++)
                {
                    crc <<= 1;

                    if ((crc & 0x10000) != 0)
                    {
                        crc = (crc ^ Poly) & 0xFFFF;
                    }
                }
            }

            ushort result = (ushort)crc;

            byte[] bytes = BitConverter.GetBytes(result);
            Array.Reverse(bytes, 0, bytes.Length);
            result = BitConverter.ToUInt16(bytes, 0);

            return result;
        }

        public static UInt128 GetDeviceId()
        {
            // FIXME: call set:cal GetAuthorId
            return SystemStateMgr.DefaultUserId.ToUInt128();
        }

        public static byte[] Ver3FacelineColorTable = new byte[] { 0, 1, 2, 3, 4, 5 };
        public static byte[] Ver3HairColorTable     = new byte[] { 8, 1, 2, 3, 4, 5, 6, 7 };
        public static byte[] Ver3EyeColorTable      = new byte[] { 8, 9, 10, 11, 12, 13 };
        public static byte[] Ver3MouthColorTable    = new byte[] { 19, 20, 21, 22, 23 };
        public static byte[] Ver3GlassColorTable    = new byte[] { 8, 14, 15, 16, 17, 18, 0 };
    }
}
