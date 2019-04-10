namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    class Operation : INode
    {
        public Instruction Inst { get; private set; }

        private Operand _dest;

        public Operand Dest
        {
            get => _dest;
            set => _dest = AssignDest(value);
        }

        private Operand[] _sources;

        public int SourcesCount => _sources.Length;

        public int ComponentIndex { get; }

        public Operation(Instruction inst, Operand dest, params Operand[] sources)
        {
            Inst     = inst;
            Dest     = dest;
            _sources = sources;

            for (int index = 0; index < sources.Length; index++)
            {
                Operand source = sources[index];

                if (source.Type == OperandType.LocalVariable)
                {
                    source.UseOps.Add(this);
                }
            }
        }

        public Operation(
            Instruction      inst,
            int              compIndex,
            Operand          dest,
            params Operand[] sources) : this(inst, dest, sources)
        {
            ComponentIndex = compIndex;
        }

        private Operand AssignDest(Operand dest)
        {
            if (dest != null && dest.Type == OperandType.LocalVariable)
            {
                dest.AsgOp = this;
            }

            return dest;
        }

        public Operand GetSource(int index)
        {
            return _sources[index];
        }

        public void SetSource(int index, Operand operand)
        {
            if (operand.Type == OperandType.LocalVariable)
            {
                operand.UseOps.Add(this);
            }

            _sources[index] = operand;
        }

        public void TurnIntoCopy(Operand source)
        {
            Inst = Instruction.Copy;

            foreach (Operand oldSrc in _sources)
            {
                if (oldSrc.Type == OperandType.LocalVariable)
                {
                    oldSrc.UseOps.Remove(this);
                }
            }

            source.UseOps.Add(this);

            _sources = new Operand[] { source };
        }
    }
}