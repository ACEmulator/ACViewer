using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ACViewer
{
    public struct VertexPositionNormalTextures : IVertexType, IEquatable<VertexPositionNormalTextures>
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

        public VertexPositionNormalTextures(Vector3 position, Vector3 normal, Vector2 uv, int textureIdx)
        {
            Position = position;
            Normal = normal;
            TextureCoord = new Vector3(uv.X, uv.Y, textureIdx);
        }

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public bool Equals(VertexPositionNormalTextures v)
        {
            return Position == v.Position && Normal == v.Normal && TextureCoord == v.TextureCoord;
        }

        public override int GetHashCode()
        {
            int hash = 0;

            hash = (hash * 397) ^ Position.GetHashCode();
            hash = (hash * 397) ^ Normal.GetHashCode();
            hash = (hash * 397) ^ TextureCoord.GetHashCode();

            return hash;
        }
    }
}
