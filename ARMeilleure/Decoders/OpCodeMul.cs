namespace ARMeilleure.Decoders
{
    class OpCodeMul : OpCodeAlu
    {
        public int Rm { get; private set; }
        public int Ra { get; private set; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeMul(inst, address, opCode);

        public OpCodeMul(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Ra = (opCode >> 10) & 0x1f;
            Rm = (opCode >> 16) & 0x1f;
        }
    }
}