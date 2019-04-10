using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeTlds : OpCodeTextureScalar
    {
        public TldsType Type => (TldsType)RawType;

        public OpCodeTlds(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode) { }
    }
}