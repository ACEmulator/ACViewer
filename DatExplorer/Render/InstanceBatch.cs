using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ACE.Server.Physics.Extensions;

namespace DatExplorer.Render
{
    public class InstanceBatch
    {
        public static GraphicsDevice GraphicsDevice { get => GameView.Instance.GraphicsDevice; }

        public Dictionary<uint, InstanceBatchDraw> DrawCalls;   // index by surface ID

        public List<VertexInstance> Instances;
        public VertexBuffer InstanceBuffer;

        public InstanceBatch(R_PhysicsObj obj)
        {
            Init();
            BuildModel(obj);
            AddInstance(obj);
        }

        public void Init()
        {
            DrawCalls = new Dictionary<uint, InstanceBatchDraw>();
            Instances = new List<VertexInstance>();
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
                var transform = setup.PlacementFrames[i];

                foreach (var polygon in part.Polygons)
                {
                    var surfaceIdx = polygon._polygon.PosSurface;
                    var surfaceID = part._gfxObj.Surfaces[surfaceIdx];

                    DrawCalls.TryGetValue(surfaceID, out var draw);

                    if (draw == null)
                    {
                        var texture = part.Textures[surfaceIdx];
                        draw = new InstanceBatchDraw(texture);
                        DrawCalls.Add(surfaceID, draw);
                    }

                    draw.AddPolygon(vertices, polygon, transform);
                }
            }
        }

        public void AddInstance(R_PhysicsObj obj)
        {
            var frame = obj.PhysicsObj.Position.Frame;
            var pos = obj.PhysicsObj.Position.GetWorldPos();
            //Console.WriteLine($"Pos: {pos}");
            var heading = frame.get_heading().ToRadians();
            var scale = obj.PhysicsObj.Scale;

            Instances.Add(new VertexInstance(pos, heading, scale));
        }

        public void OnCompleted()
        {
            BuildInstanceBuffer();
            BuildBindings();
        }

        public void BuildInstanceBuffer()
        {
            InstanceBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexInstance), Instances.Count, BufferUsage.WriteOnly);
            InstanceBuffer.SetData(Instances.ToArray());
        }

        public void BuildBindings()
        {
            foreach (var batch in DrawCalls.Values)
                batch.OnCompleted(InstanceBuffer);
        }

        public void Draw()
        {
            foreach (var drawCall in DrawCalls.Values)
                drawCall.Draw(Instances.Count);
        }

        public void Dispose()
        {
            foreach (var batch in DrawCalls.Values)
                batch.Dispose();

            InstanceBuffer.Dispose();
        }
    }
}
