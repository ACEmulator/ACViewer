using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework.Graphics;

namespace ACViewer.Render
{
    public class TerrainBatch
    {
        public static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public static Effect Effect => Render.Effect;

        public EffectParameters EffectParameters { get; set; }

        public List<LandVertex> Vertices { get; set; }

        public VertexBuffer VertexBuffer { get; set; }

        public int NumItems { get; set; }

        public TerrainBatch()
        {
            Init();
        }

        public void Init()
        {
            EffectParameters = new EffectParameters();
            Vertices = new List<LandVertex>();
        }

        public TerrainBatch(uint surfnum)
        {
            Init();

            EffectParameters.Overlays = R_Landblock.LandOverlays[surfnum];
            EffectParameters.Alphas = R_Landblock.LandAlphas[surfnum];
        }

        public void AddCell(List<LandVertex> vertices, int polyIdx)
        {
            Vertices.AddRange(vertices.Skip(polyIdx * 3).Take(6));
        }

        public void BuildBuffer()
        {
            VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(LandVertex), Vertices.Count, BufferUsage.WriteOnly);
            VertexBuffer.SetData(Vertices.ToArray());

            NumItems = Vertices.Count / 3;
        }

        public void Draw()
        {
            GraphicsDevice.SetVertexBuffer(VertexBuffer);

            Effect.Parameters["xOverlays"].SetValue(EffectParameters.Overlays);
            Effect.Parameters["xAlphas"].SetValue(EffectParameters.Alphas);

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
