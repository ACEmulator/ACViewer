using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DatExplorer
{
    public static class VertexExtensions
    {
        public static VertexPositionNormalTexture Transform(this VertexPositionNormalTexture _v, Matrix transform)
        {
            return new VertexPositionNormalTexture(Vector3.Transform(_v.Position, transform), _v.Normal, _v.TextureCoordinate);
        }
    }
}
