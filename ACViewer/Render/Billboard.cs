using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ACViewer.Render
{
    public static class Billboard
    {
        public static List<VertexPositionTexture> Vertices;
        public static List<short> Indices;

        public static VertexBuffer VertexBuffer;
        public static IndexBuffer IndexBuffer;

        static Billboard()
        {
            Vertices = new List<VertexPositionTexture>();
            Vertices.Add(new VertexPositionTexture(Vector3.Zero, new Vector2(0, 1)));
            Vertices.Add(new VertexPositionTexture(Vector3.Zero, new Vector2(1, 1)));
            Vertices.Add(new VertexPositionTexture(Vector3.Zero, new Vector2(0, 0)));
            Vertices.Add(new VertexPositionTexture(Vector3.Zero, new Vector2(1, 0)));

            Indices = new List<short>() { 0, 1, 2, 3 };

            VertexBuffer = new VertexBuffer(GameView.Instance.GraphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
            VertexBuffer.SetData(Vertices.ToArray());

            IndexBuffer = new IndexBuffer(GameView.Instance.GraphicsDevice, typeof(short), 4, BufferUsage.WriteOnly);
            IndexBuffer.SetData(Indices.ToArray());
        }
    }
}
