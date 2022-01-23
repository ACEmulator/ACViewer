using System;

using Microsoft.Xna.Framework.Graphics;

using ACE.Entity.Enum;

namespace ACViewer.Render
{
    public class ParticleTextureFormat : IEquatable<ParticleTextureFormat>
    {
        public SurfaceFormat SurfaceFormat { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsAdditive { get; set; }

        public ParticleTextureFormat(SurfaceFormat surfaceFormat, SurfaceType surfaceType, int width, int height)
        {
            SurfaceFormat = surfaceFormat;
            //SurfaceType = surfaceType;
            Width = width;
            Height = height;
            IsAdditive = surfaceType.HasFlag(SurfaceType.Additive);
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

        public bool Equals(ParticleTextureFormat textureFormat)
        {
            return SurfaceFormat == textureFormat.SurfaceFormat && Width == textureFormat.Width && Height == textureFormat.Height && IsAdditive == textureFormat.IsAdditive;
        }

        public override int GetHashCode()
        {
            var hash = 0;

            hash = (hash * 397) ^ SurfaceFormat.GetHashCode();
            //hash = (hash * 397) ^ SurfaceType.GetHashCode();
            hash = (hash * 397) ^ Width.GetHashCode();
            hash = (hash * 397) ^ Height.GetHashCode();
            hash = (hash * 397) ^ IsAdditive.GetHashCode();

            return hash;
        }

        public override string ToString()
        {
            return $"SurfaceFormat: {SurfaceFormat}, Width: {Width}, Height: {Height}, IsAdditive: {IsAdditive}";
        }
    }
}
