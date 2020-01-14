﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ARMeilleure.Decoders
{
    class OpCode32SimdTbl : OpCode32SimdReg
    {
        public int Length { get; private set; }
        public OpCode32SimdTbl(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Length = (opCode >> 8) & 3;
            Size = 0;
            Opc = Q ? 1 : 0;
            Q = false;
            RegisterSize = RegisterSize.Simd64;
        }
    }
}
