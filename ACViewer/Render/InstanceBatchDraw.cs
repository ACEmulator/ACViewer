using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ACViewer.Model;

namespace ACViewer.Render
{
    public class InstanceBatchDraw
    {
        public static GraphicsDevice GraphicsDevice { get => GameView.Instance.GraphicsDevice; }

        public static Effect Effect { get => Render.Effect; }

        public EffectParameters EffectParameters;

        //public List<VertexPositionNormalTexture> Vertices;
        public List<VertexPositionNormalTextures> Vertices;

        public VertexBuffer VertexBuffer;
        public IndexBuffer IndexBuffer;

        public TextureFormat TextureFormat;
        public Dictionary<uint, byte> TextureIndex;

        public int NumItems;

        public VertexBufferBinding[] Bindings;

        public InstanceBatchDraw()
        {
            Init();
        }

        public void Init()
        {
            EffectParameters = new EffectParameters();
            //Vertices = new List<VertexPositionNormalTexture>();
            Vertices = new List<VertexPositionNormalTextures>();
            TextureIndex = new Dictionary<uint, byte>();
        }

        public InstanceBatchDraw(Texture2D texture)
        {
            Init();

            EffectParameters.Texture = texture;
        }

        public InstanceBatchDraw(TextureFormat textureFormat)
        {
            Init();

            TextureFormat = textureFormat;
        }

        public void AddPolygon(List<VertexPositionNormalTexture> vertices, Polygon polygon, Matrix world)
        {
            //foreach (var idx in polygon.Indices)
            //Vertices.Add(vertices[idx].Transform(world));
        }

        public void AddPolygon(List<VertexPositionNormalTexture> vertices, Polygon polygon, uint textureID, Matrix model)
        {
            if (!TextureIndex.TryGetValue(textureID, out var textureIdx))
            {
                textureIdx = (byte)TextureIndex.Count;
                TextureIndex.Add(textureID, textureIdx);
            }

            foreach (var idx in polygon.Indices)
                Vertices.Add(vertices[idx].Transform(model, textureIdx));
        }

        public void BuildTextures()
        {
            var bytesPerPixel = TextureFormat.GetBytesPerPixel();

            var textures = new Texture2D(GraphicsDevice, TextureFormat.Width, TextureFormat.Height, false, TextureFormat.SurfaceFormat, TextureIndex.Count);
            foreach (var kvp in TextureIndex)
            {
                var surface = kvp.Key;
                var idx = kvp.Value;
                var texture = TextureCache.Get(surface);
                //Console.WriteLine($"Texture format: {texture.Format}");
                var data = new byte[(int)(texture.Width * texture.Height * bytesPerPixel)];
                texture.GetData(data);
                textures.SetData(0, idx, null, data, 0, data.Length);
            }
            EffectParameters.Texture = textures;
        }

        public void OnCompleted(VertexBuffer instanceBuffer)
        {
            BuildBuffer();
            BuildBindings(instanceBuffer);
        }

        public void BuildBuffer()
        {
            //VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), Vertices.Count, BufferUsage.WriteOnly);
            VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTextures), Vertices.Count, BufferUsage.WriteOnly);
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

            Effect.Parameters["xTextures"].SetValue(EffectParameters.Texture);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, NumItems, numInstances);
            }
        }

        public void Dispose()
        {
            EffectParameters.Dispose();
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }
    }
}
