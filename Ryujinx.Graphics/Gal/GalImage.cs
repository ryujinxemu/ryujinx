using Ryujinx.Graphics.Texture;

namespace Ryujinx.Graphics.Gal
{
    public struct GalImage
    {
        public int Width;
        public int Height;
        public int Depth;
        public int TileWidth;
        public int GobBlockHeight;
        public int Pitch;
        public int MaxMipmapLevel;

        public GalImageFormat   Format;
        public GalMemoryLayout  Layout;
        public GalTextureSource XSource;
        public GalTextureSource YSource;
        public GalTextureSource ZSource;
        public GalTextureSource WSource;
        public GalTextureTarget TextureTarget;

        public GalImage(
            int              Width,
            int              Height,
            int              Depth,
            int              TileWidth,
            int              GobBlockHeight,
            GalMemoryLayout  Layout,
            GalImageFormat   Format,
            GalTextureTarget TextureTarget,
            int              MaxMipmapLevel = 1,
            GalTextureSource XSource        = GalTextureSource.Red,
            GalTextureSource YSource        = GalTextureSource.Green,
            GalTextureSource ZSource        = GalTextureSource.Blue,
            GalTextureSource WSource        = GalTextureSource.Alpha)
        {
            this.Width          = Width;
            this.Height         = Height;
            this.Depth          = Depth;
            this.TileWidth      = TileWidth;
            this.GobBlockHeight = GobBlockHeight;
            this.Layout         = Layout;
            this.Format         = Format;
            this.MaxMipmapLevel = MaxMipmapLevel;
            this.XSource        = XSource;
            this.YSource        = YSource;
            this.ZSource        = ZSource;
            this.WSource        = WSource;
            this.TextureTarget  = TextureTarget;

            Pitch = ImageUtils.GetPitch(Format, Width);
        }

        public bool SizeMatches(GalImage Image)
        {
            if (ImageUtils.GetBytesPerPixel(Format) !=
                ImageUtils.GetBytesPerPixel(Image.Format))
            {
                return false;
            }

            if (ImageUtils.GetAlignedWidth(this) !=
                ImageUtils.GetAlignedWidth(Image))
            {
                return false;
            }

            return Height == Image.Height;
        }
    }
}