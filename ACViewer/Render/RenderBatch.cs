using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ACViewer.Model;

namespace ACViewer.Render
{
    public class RenderBatch
    {
        public static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public static Effect Effect => Render.Effect;

        public EffectParameters EffectParameters { get; set; }

        public List<VertexPositionNormalTexture> Vertices { get; set; }

        public VertexBuffer VertexBuffer { get; set; }

        public int NumItems { get; set; }

        public RenderBatch()
        {
            Init();
        }

        public void Init()
        {
            EffectParameters = new EffectParameters();
            Vertices = new List<VertexPositionNormalTexture>();
        }

        public RenderBatch(Texture2D texture)
        {
            Init();

            EffectParameters.Texture = texture;
        }

        public void AddPolygon(List<VertexPositionNormalTexture> vertices, Polygon polygon, Matrix world)
        {
            foreach (var idx in polygon.Indices)
                Vertices.Add(vertices[idx].Transform(world));
        }

        public void BuildBuffer()
        {
            VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), Vertices.Count, BufferUsage.WriteOnly);
            VertexBuffer.SetData(Vertices.ToArray());

            NumItems = Vertices.Count / 3;
        }

        public void Draw()
        {
            GraphicsDevice.SetVertexBuffer(VertexBuffer);

            Effect.Parameters["xTextures"].SetValue(EffectParameters.Texture);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, NumItems);
            }
        }

        public void Dispose()
        {
            EffectParameters.Dispose();
            VertexBuffer.Dispose();
        }
    }
}
