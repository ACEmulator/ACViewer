using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DatExplorer.Model;

namespace DatExplorer.Render
{
    public class InstanceBatchDraw
    {
        public static GraphicsDevice GraphicsDevice { get => GameView.Instance.GraphicsDevice; }

        public static Effect Effect { get => Render.Effect; }

        public EffectParameters EffectParameters;

        public List<VertexPositionNormalTexture> Vertices;

        public VertexBuffer VertexBuffer;
        public IndexBuffer IndexBuffer;

        public int NumItems;

        public VertexBufferBinding[] Bindings;

        public InstanceBatchDraw()
        {
            Init();
        }

        public void Init()
        {
            EffectParameters = new EffectParameters();
            Vertices = new List<VertexPositionNormalTexture>();
        }

        public InstanceBatchDraw(Texture2D texture)
        {
            Init();

            EffectParameters.Texture = texture;
        }

        public void AddPolygon(List<VertexPositionNormalTexture> vertices, Polygon polygon, Matrix setupModel)
        {
            foreach (var idx in polygon.Indices)
                Vertices.Add(vertices[idx].Transform(setupModel));
        }

        public void OnCompleted(VertexBuffer instanceBuffer)
        {
            BuildBuffer();
            BuildBindings(instanceBuffer);
        }

        public void BuildBuffer()
        {
            VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), Vertices.Count, BufferUsage.WriteOnly);
            VertexBuffer.SetData(Vertices.ToArray());

            var indices = new ushort[Vertices.Count];
            for (ushort i = 0; i < Vertices.Count; i++)
                indices[i] = i;

            IndexBuffer = new IndexBuffer(GraphicsDevice, typeof(ushort), Vertices.Count, BufferUsage.WriteOnly);
            IndexBuffer.SetData(indices);

            NumItems = Vertices.Count / 3;
        }

        public void BuildBindings(VertexBuffer instanceBuffer)
        {
            Bindings = new VertexBufferBinding[2];
            Bindings[0] = new VertexBufferBinding(VertexBuffer);
            Bindings[1] = new VertexBufferBinding(instanceBuffer, 0, 1);
        }

        public void Draw(int numInstances)
        {
            GraphicsDevice.SetVertexBuffers(Bindings);
            GraphicsDevice.Indices = IndexBuffer;

            Effect.Parameters["xTexture"].SetValue(EffectParameters.Texture);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, NumItems, numInstances);
            }
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }
    }
}
