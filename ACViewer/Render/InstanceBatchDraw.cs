using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

using ACViewer.Model;

namespace ACViewer.Render
{
    public class InstanceBatchDraw
    {
        public static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public static Effect Effect => Render.Effect;

        public static Effect Effect_Clamp => Render.Effect_Clamp;

        public Texture2D Textures { get; set; }

        public List<VertexPositionNormalTextures> Vertices { get; set; }

        public VertexBuffer VertedBuffer { get; set; }
        public IndexBuffer IndexBuffer { get; set; }

        public TextureFormat TextureFormat { get; set; }
        public Dictionary<uint, byte> TextureIndex { get; set; }

        public int NumItems { get; set; }

        public VertexBufferBinding[] Bindings { get; set; }

        public InstanceBatchDraw(TextureFormat textureFormat)
        {
            TextureFormat = textureFormat;

            Vertices = new List<VertexPositionNormalTextures>();

            TextureIndex = new Dictionary<uint, byte>();
        }

        public void AddPolygon(List<VertexPositionNormalTexture> vertices, Polygon polygon, uint textureID)
        {
            if (!TextureIndex.TryGetValue(textureID, out var textureIdx))
            {
                textureIdx = (byte)TextureIndex.Count;
                TextureIndex.Add(textureID, textureIdx);
            }

            foreach (var idx in polygon.Indices)
            {
                var v = vertices[idx];
                Vertices.Add(new VertexPositionNormalTextures(v.Position, v.Normal, v.TextureCoordinate, textureIdx));
            }
        }

        public void BuildTextures()
        {
            var bytesPerPixel = TextureFormat.GetBytesPerPixel();

            Textures = new Texture2D(GraphicsDevice, TextureFormat.Width, TextureFormat.Height, false, TextureFormat.SurfaceFormat, TextureIndex.Count);

            foreach (var kvp in TextureIndex)
            {
                var surface = kvp.Key;
                var idx = kvp.Value;
                var texture = TextureCache.Get(surface);
                //Console.WriteLine($"Texture format: {texture.Format}");
                var data = new byte[(int)(texture.Width * texture.Height * bytesPerPixel)];
                texture.GetData(data);
                Textures.SetData(0, idx, null, data, 0, data.Length);
            }
        }

        public void OnCompleted(VertexBuffer instanceBuffer)
        {
            BuildBuffer();
            BuildBindings(instanceBuffer);
        }

        public void BuildBuffer()
        {
            VertedBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTextures), Vertices.Count, BufferUsage.WriteOnly);
            VertedBuffer.SetData(Vertices.ToArray());

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
            Bindings[0] = new VertexBufferBinding(VertedBuffer);
            Bindings[1] = new VertexBufferBinding(instanceBuffer, 0, 1);
        }

        public void Draw(int numInstances)
        {
            GraphicsDevice.SetVertexBuffers(Bindings);
            GraphicsDevice.Indices = IndexBuffer;

            var effect = TextureFormat.HasWrappingUVs ? Effect : Effect_Clamp;

            effect.Parameters["xTextures"].SetValue(Textures);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, NumItems, numInstances);
            }
        }

        public void Dispose()
        {
            Textures.Dispose();
            VertedBuffer.Dispose();
            IndexBuffer.Dispose();
        }
    }
}
