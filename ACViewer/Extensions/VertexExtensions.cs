using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ACViewer
{
    public static class VertexExtensions
    {
        public static VertexPositionNormalTexture Transform(this VertexPositionNormalTexture _v, Matrix transform)
        {
            return new VertexPositionNormalTexture(Vector3.Transform(_v.Position, transform), _v.Normal, _v.TextureCoordinate);
        }

        public static VertexPositionNormalTextures Transform(this VertexPositionNormalTexture _v, Matrix transform, byte textureIdx)
        {
            return new VertexPositionNormalTextures(Vector3.Transform(_v.Position, transform), _v.Normal, _v.TextureCoordinate, textureIdx);
        }
    }
}
