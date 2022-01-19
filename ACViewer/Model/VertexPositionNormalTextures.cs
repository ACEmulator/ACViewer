using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ACViewer
{
    public struct VertexPositionNormalTextures : IVertexType
    {
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public Vector3 TextureCoord { get; set; }

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        );

        public VertexPositionNormalTextures(Vector3 position, Vector3 normal, Vector2 uv, byte textureIdx)
        {
            Position = position;
            Normal = normal;
            TextureCoord = new Vector3(uv.X, uv.Y, textureIdx);
        }

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }
}
