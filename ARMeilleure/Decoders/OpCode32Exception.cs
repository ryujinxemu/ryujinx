﻿namespace ARMeilleure.Decoders
{
    class OpCode32Exception : OpCode32
    {
        public int Id { get; private set; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32Exception(inst, address, opCode);

        public OpCode32Exception(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Id = opCode & 0xFFFFFF;
        }
    }
}
