using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ACViewer
{
    public struct LandVertex : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 TexCoord0;  // base uv, texture idx
        public Vector4 TexCoord1;  // overlay 1 uv, texture idx, alpha idx
        public Vector4 TexCoord2;  // overlay 2 uv, texture idx, alpha idx
        public Vector4 TexCoord3;  // overlay 3 uv, texture idx, alpha idx
        public Vector4 TexCoord4;  // road 1 uv, texture idx, alpha idx
        public Vector4 TexCoord5;  // road 2 uv, texture idx, alpha idx

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 9, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(sizeof(float) * 13, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
            new VertexElement(sizeof(float) * 17, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
            new VertexElement(sizeof(float) * 21, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4),
            new VertexElement(sizeof(float) * 25, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 5)
        );

        public LandVertex(bool dummy)
        {
            Position = Vector3.Zero;
            Normal = Vector3.Zero;
            TexCoord0 = Vector3.Zero;
            TexCoord1 = -Vector4.One;
            TexCoord2 = -Vector4.One;
            TexCoord3 = -Vector4.One;
            TexCoord4 = -Vector4.One;
            TexCoord5 = -Vector4.One;
        }

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }
}
