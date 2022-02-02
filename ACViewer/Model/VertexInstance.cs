using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ACViewer.Model
{
    public struct VertexInstance : IVertexType
    {
        public Vector3 Position;
        public Vector4 Orientation;
        public Vector3 Scale;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(sizeof(float) * 7, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 2)
        );

        public VertexInstance(Vector3 position, Quaternion orientation, Vector3 scale)
        {
            Position = position;
            Orientation = new Vector4(orientation.X, orientation.Y, orientation.Z, orientation.W);
            Scale = scale;
        }

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }
}
