using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

using ACViewer.Model;
using ACViewer.Render;

namespace ACViewer
{
    public class GfxObjInstance_TextureFormat
    {
        // split up by TextureFormat

        // this isn't an actual instance, but rather the set of Polygons in a GfxObj for a TextureFormat
        public static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public Effect Effect { get; set; }

        public TextureAtlas TextureAtlas { get; set; }

        public List<short> Indices { get; set; }

        public IndexBuffer IndexBuffer { get; set; }

        public int NumItems { get; set; }

        public GfxObjInstance_TextureFormat(TextureAtlas textureAtlas)
        {
            TextureAtlas = textureAtlas;

            Indices = new List<short>();

            if (textureAtlas.TextureFormatChain.TextureFormat.HasWrappingUVs)
                Effect = Render.Render.Effect;
            else
                Effect = Render.Render.Effect_Clamp;
        }

        public void AddPolygon(Polygon polygon, List<VertexPositionNormalTexture> vertices, uint surfaceID, List<VertexPositionNormalTextures> outVertices, Dictionary<VertexPositionNormalTextures, short> vertexTable, int textureIdx)
        {
            foreach (var idx in polygon.Indices)
            {
                var v = new VertexPositionNormalTextures(vertices[idx].Position, vertices[idx].Normal, vertices[idx].TextureCoordinate, textureIdx);

                if (!vertexTable.TryGetValue(v, out var existingIdx))
                {
                    existingIdx = (short)outVertices.Count;
                    vertexTable.Add(v, existingIdx);
                    outVertices.Add(v);
                }
                Indices.Add(existingIdx);
            }
        }

        public void BuildBuffer()
        {
            if (Indices.Count == 0) return;

            IndexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), Indices.Count, BufferUsage.WriteOnly);
            IndexBuffer.SetData(Indices.ToArray());

            NumItems = Indices.Count / 3;
        }

        public void Draw(int numInstances)
        {
            if (IndexBuffer == null) return;

            GraphicsDevice.Indices = IndexBuffer;

            Effect.Parameters["xTextures"].SetValue(TextureAtlas._Textures);

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, NumItems, numInstances);
            }
        }

        public void Dispose()
        {
            if (IndexBuffer != null)
                IndexBuffer.Dispose();
        }
    }
}
