using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ACViewer
{
    public struct VertexInstance : IVertexType
    {
        public Vector3 Position { get; set; }
        public Vector2 HeadingScale { get; set; }

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1)  // heading, scale
        );

        public VertexInstance(Vector3 position, float rotation = 0.0f, float scale = 1.0f)
        {
            Position = position;
            HeadingScale = new Vector2(rotation, scale);
        }

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }
}
