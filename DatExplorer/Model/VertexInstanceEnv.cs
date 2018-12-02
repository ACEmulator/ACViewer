﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DatExplorer
{
    public struct VertexInstanceEnv : IVertexType
    {
        public Vector3 Position;
        public Vector4 Rotation;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1)
        );

        public VertexInstanceEnv(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = new Vector4(rotation.X, rotation.Y, rotation.Z, rotation.W);
        }

        VertexDeclaration IVertexType.VertexDeclaration { get => VertexDeclaration; }
    }
}
