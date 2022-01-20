using System;

using Microsoft.Xna.Framework.Graphics;

namespace ACViewer.Render
{
    public class TextureFormat: IEquatable<TextureFormat>
    {
        public SurfaceFormat SurfaceFormat { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public TextureFormat(SurfaceFormat surfaceFormat, int width, int height)
        {
            SurfaceFormat = surfaceFormat;
            Width = width;
            Height = height;
        }

        public float GetBytesPerPixel()
        {
            switch (SurfaceFormat)
            {
                case SurfaceFormat.Dxt1:
                    return 0.5f;
                case SurfaceFormat.Color:
                    return 4.0f;
                default:
                    return 1.0f;
            }
        }

        public bool Equals(TextureFormat textureFormat)
        {
            return SurfaceFormat == textureFormat.SurfaceFormat && Width == textureFormat.Width && Height == textureFormat.Height;
        }

        public override int GetHashCode()
        {
            var hash = 0;

            hash = (hash * 397) ^ SurfaceFormat.GetHashCode();
            hash = (hash * 397) ^ Width.GetHashCode();
            hash = (hash * 397) ^ Height.GetHashCode();

            return hash;
        }
    }
}
