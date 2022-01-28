using System;

using Microsoft.Xna.Framework.Graphics;

namespace ACViewer.Render
{
    public class TextureFormat : IEquatable<TextureFormat>
    {
        public SurfaceFormat SurfaceFormat { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool HasWrappingUVs { get; set; }

        public TextureFormat(SurfaceFormat surfaceFormat, int width, int height, bool hasWrappingUVs)
        {
            SurfaceFormat = surfaceFormat;
            Width = width;
            Height = height;
            HasWrappingUVs = hasWrappingUVs;
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
            return SurfaceFormat == textureFormat.SurfaceFormat && Width == textureFormat.Width && Height == textureFormat.Height && HasWrappingUVs == textureFormat.HasWrappingUVs;
        }

        public override int GetHashCode()
        {
            var hash = 0;

            hash = (hash * 397) ^ SurfaceFormat.GetHashCode();
            hash = (hash * 397) ^ Width.GetHashCode();
            hash = (hash * 397) ^ Height.GetHashCode();
            hash = (hash * 397) ^ HasWrappingUVs.GetHashCode();

            return hash;
        }

        public override string ToString()
        {
            return $"SurfaceFormat: {SurfaceFormat}, Width: {Width}, Height: {Height}, HasWrappingUVs: {HasWrappingUVs}";
        }
    }
}
