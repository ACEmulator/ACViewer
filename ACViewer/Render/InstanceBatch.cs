using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

namespace ACViewer.Render
{
    public class InstanceBatch
    {
        public static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public Dictionary<TextureFormat, InstanceBatchDraw> DrawCalls { get; set; }

        public List<VertexInstanceEnv> Instances_Env { get; set; }

        public VertexBuffer InstanceBuffer { get; set; }

        public R_Environment R_Environment { get; set; }

        public InstanceBatch(R_EnvCell envCell)
        {
            Init();

            BuildModel(envCell);
            BuildTextures();

            AddInstance(envCell);

            R_Environment = envCell.Environment;
        }

        public void Init()
        {
            DrawCalls = new Dictionary<TextureFormat, InstanceBatchDraw>();

            Instances_Env = new List<VertexInstanceEnv>();
        }

        public void BuildModel(R_EnvCell envCell)
        {
            foreach (var cellStruct in envCell.Environment.R_CellStructs.Values)
            {
                var vertices = cellStruct.VertexArray;

                foreach (var polygon in cellStruct.Polygons)
                {
                    var surfaceIdx = polygon._polygon.PosSurface;
                    var surfaceID = envCell.EnvCell._envCell.Surfaces[surfaceIdx];

                    var texture = TextureCache.Get(surfaceID);
                    //Console.WriteLine($"Texture: {surfaceID:X8} Size: {texture.Width}x{texture.Height}");
                    var textureFormat = new TextureFormat(texture.Format, texture.Width, texture.Height, cellStruct.HasWrappingUVs);

                    if (!DrawCalls.TryGetValue(textureFormat, out var batch))
                    {
                        batch = new InstanceBatchDraw(textureFormat);
                        DrawCalls.Add(textureFormat, batch);
                    }
                    batch.AddPolygon(vertices, polygon, surfaceID);
                }
            }
        }

        public void BuildTextures()
        {
            foreach (var drawCall in DrawCalls.Values)
                drawCall.BuildTextures();
        }

        public void AddInstance(R_EnvCell envCell)
        {
            var origin = envCell.EnvCell.Pos.GetWorldPos();
            var orientation = envCell.EnvCell.Pos.Frame.Orientation.ToXna();

            origin.Z += 0.05f;     // avoid z-fight for building floors

            Instances_Env.Add(new VertexInstanceEnv(origin, orientation));
        }

        public void OnCompleted()
        {
            BuildInstanceBuffer();
            BuildBindings();
        }

        public void BuildInstanceBuffer()
        {
            InstanceBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexInstanceEnv), Instances_Env.Count, BufferUsage.WriteOnly);
            InstanceBuffer.SetData(Instances_Env.ToArray());
        }

        public void BuildBindings()
        {
            foreach (var batch in DrawCalls.Values)
                batch.OnCompleted(InstanceBuffer);
        }

        public void Draw()
        {
            foreach (var drawCall in DrawCalls.Values)
                drawCall.Draw(Instances_Env.Count);
        }

        public void Dispose()
        {
            foreach (var batch in DrawCalls.Values)
                batch.Dispose();

            InstanceBuffer.Dispose();
        }
    }
}
