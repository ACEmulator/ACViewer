using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ACViewer
{
    public struct LandVertex: IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 TexCoord0;
        public Vector3 TexCoord1;
        public Vector3 TexCoord2;
        public Vector3 TexCoord3;
        public Vector3 TexCoord4;
        public Vector3 TexCoord5;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 9, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(sizeof(float) * 12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 2),
            new VertexElement(sizeof(float) * 15, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 3),
            new VertexElement(sizeof(float) * 18, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 4),
            new VertexElement(sizeof(float) * 21, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 5)
        );

        VertexDeclaration IVertexType.VertexDeclaration { get => VertexDeclaration; }
    }
}
