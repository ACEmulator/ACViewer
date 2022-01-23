using System.Numerics;

using Microsoft.Xna.Framework.Graphics;

namespace ACViewer
{
    public struct ParticleDeclaration : IVertexType
    {
        // per-particle data
        // align to 16?

        public Vector3 Position;

        public Vector3 BillboardTexture;   // pointSpriteSizeX = u, pointSpriteSizeY = v, textureIdx = w
        public Vector3 ScaleOpacityActive; // scale = x, opacity = y, active = z

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            // VertexElementUsage.PointSize for base?
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector3, VertexElementUsage.Position, 2)
        );

        public ParticleDeclaration(int textureIdx, Vector2 dims)
        {
            Position = Vector3.Zero;
            BillboardTexture = new Vector3(dims.X, dims.Y, textureIdx);
            ScaleOpacityActive = Vector3.Zero;
        }

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }
}
