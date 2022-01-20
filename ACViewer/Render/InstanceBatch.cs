using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ACE.Server.Physics.Extensions;

namespace ACViewer.Render
{
    public class InstanceBatch
    {
        public static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public Dictionary<TextureFormat, InstanceBatchDraw> DrawCalls { get; set; }

        public List<VertexInstance> Instances { get; set; }
        public List<VertexInstanceEnv> Instances_Env { get; set; }

        public VertexBuffer InstanceBuffer { get; set; }

        public R_PhysicsObj R_PhysicsObj { get; set; }
        public R_Environment R_Environment { get; set; }

        public InstanceBatch(R_PhysicsObj obj)
        {
            Init();

            BuildModel(obj);
            BuildTextures();

            AddInstance(obj);

            R_PhysicsObj = obj;
        }

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
            Instances = new List<VertexInstance>();
            Instances_Env = new List<VertexInstanceEnv>();
        }

        public void BuildModel(R_PhysicsObj obj)
        {
            var setupInstance = obj.Setup;
            var setup = setupInstance.Setup;

            if (setup.Parts.Count != obj.PartArray.Parts.Count) return;

            for (var i = 0; i < setup.Parts.Count; i++)
            {
                var part = setup.Parts[i];
                var vertices = part.VertexArray;

                var frame = obj.PartArray.Parts[i].PhysicsPart.Pos;
                var transform = setup.PlacementFrames[i];

                foreach (var polygon in part.Polygons)
                {
                    var surfaceIdx = polygon._polygon.PosSurface;
                    var surfaceID = part._gfxObj.Surfaces[surfaceIdx];

                    var texture = TextureCache.Get(surfaceID);
                    //Console.WriteLine($"Texture: {surfaceID:X8} Size: {texture.Width}x{texture.Height}");
                    var textureFormat = new TextureFormat(texture.Format, texture.Width, texture.Height);

                    DrawCalls.TryGetValue(textureFormat, out var batch);
                    if (batch == null)
                    {
                        batch = new InstanceBatchDraw(textureFormat);
                        DrawCalls.Add(textureFormat, batch);
                    }
                    batch.AddPolygon(vertices, polygon, surfaceID, transform);
                }
            }
        }

        public void BuildModel(R_EnvCell envCell)
        {
            var transform = Matrix.Identity;
            var env = envCell.Environment;

            foreach (var cellStruct in env.R_CellStructs.Values)
            {
                var vertices = cellStruct.VertexArray;

                foreach (var polygon in cellStruct.Polygons)
                {
                    var surfaceIdx = polygon._polygon.PosSurface;
                    var surfaceID = envCell.EnvCell._envCell.Surfaces[surfaceIdx];

                    var texture = TextureCache.Get(surfaceID);
                    //Console.WriteLine($"Texture: {surfaceID:X8} Size: {texture.Width}x{texture.Height}");
                    var textureFormat = new TextureFormat(texture.Format, texture.Width, texture.Height);

                    DrawCalls.TryGetValue(textureFormat, out var batch);
                    if (batch == null)
                    {
                        batch = new InstanceBatchDraw(textureFormat);
                        DrawCalls.Add(textureFormat, batch);
                    }
                    batch.AddPolygon(vertices, polygon, surfaceID, transform);
                }
            }
        }

        public void BuildTextures()
        {
            foreach (var drawCall in DrawCalls.Values)
                drawCall.BuildTextures();
        }

        public void AddInstance(R_PhysicsObj obj)
        {
            var _pos = obj.PhysicsObj.Position;
            var pos = _pos.GetWorldPos();
            var heading = _pos.Frame.get_heading().ToRadians();
            var scale = obj.PhysicsObj.Scale;

            Instances.Add(new VertexInstance(pos, heading, scale));
        }

        public void AddInstance(R_EnvCell envCell)
        {
            var _pos = envCell.EnvCell.Pos;
            var pos = _pos.GetWorldPos();
            pos.Z += 0.05f;     // avoid z-fight for building floors
            var rotation = _pos.Frame.Orientation.ToXna();

            Instances_Env.Add(new VertexInstanceEnv(pos, rotation));
        }

        public void OnCompleted()
        {
            BuildInstanceBuffer();
            BuildBindings();
        }

        public void BuildInstanceBuffer()
        {
            if (R_Environment == null)
            {
                InstanceBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexInstance), Instances.Count, BufferUsage.WriteOnly);
                InstanceBuffer.SetData(Instances.ToArray());
            }
            else
            {
                InstanceBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexInstanceEnv), Instances_Env.Count, BufferUsage.WriteOnly);
                InstanceBuffer.SetData(Instances_Env.ToArray());
            }
        }

        public void BuildBindings()
        {
            foreach (var batch in DrawCalls.Values)
                batch.OnCompleted(InstanceBuffer);
        }

        public void Draw()
        {
            if (R_Environment == null)
            {
                foreach (var drawCall in DrawCalls.Values)
                    drawCall.Draw(Instances.Count);
            }
            else
            {
                foreach (var drawCall in DrawCalls.Values)
                    drawCall.Draw(Instances_Env.Count);
            }
        }

        public void Dispose()
        {
            foreach (var batch in DrawCalls.Values)
                batch.Dispose();

            InstanceBuffer.Dispose();
        }
    }
}
