using System;
using System.Collections.Generic;
using System.Numerics;

using Microsoft.Xna.Framework.Graphics;

using ACE.Entity.Enum;
using ACE.Server.Physics;

using ACViewer.Model;

namespace ACViewer.Render
{
    public class ParticleBatch
    {
        public static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public Dictionary<ParticleTextureFormat, ParticleBatchDraw> DrawCalls { get; set; }

        public ParticleBatch()
        {
            Init();
        }

        public void Init()
        {
            DrawCalls = new Dictionary<ParticleTextureFormat, ParticleBatchDraw>();
        }

        private static readonly float pointSpriteSize = 1.8f;   // guessing

        public void AddEmitter(ParticleEmitter emitter)
        {
            // get the texture for this gfxobj
            var gfxObj = GfxObjCache.Get(emitter.Info.HWGfxObjID);
            var texture = gfxObj.Textures[0];
            var surfaceType = gfxObj.Surfaces[0].Type;

            if (gfxObj.Polygons.Count != 1)
            {
                Console.WriteLine($"ParticleBatch.AddEmitter({emitter.Info._info.Id:X8}) - Polygons.Count for {gfxObj._gfxObj.Id:X8} == {gfxObj.Polygons.Count}, expected 1!");
                return;
            }

            var textureFormat = new ParticleTextureFormat(texture.Format, surfaceType, texture.Width, texture.Height);

            if (!DrawCalls.TryGetValue(textureFormat, out var batch))
            {
                batch = new ParticleBatchDraw(textureFormat);
                DrawCalls.Add(textureFormat, batch);
            }

            var polygon = gfxObj.Polygons[0];

            var surfaceIdx = polygon._polygon.PosSurface;
            var surfaceID = gfxObj._gfxObj.Surfaces[surfaceIdx];

            // PlacementFrames transform omitted, since it contains none from SimpleSetup

            // basically we want to add an instance for each of the possible particle slots,
            // and a mapping from each particle slot -> the render instance

            if (gfxObj.BoundingBox == null)
            {
                gfxObj.BuildBoundingBox();

                //if (gfxObj.BoundingBox.Size.Y != 0)
                    //Console.WriteLine($"Warning: Found particle BoundingBox where Size.Y > 0 for {gfxObj._gfxObj.Id:X8}");

                //Console.WriteLine($"{gfxObj._gfxObj.Id:X8} bbox: {gfxObj.BoundingBox.Mins} - {gfxObj.BoundingBox.Maxs}");
            }

            var dims = new Vector2(gfxObj.BoundingBox.Size.X * pointSpriteSize, gfxObj.BoundingBox.Size.Z * pointSpriteSize);

            for (var i = 0; i < emitter.Info.MaxParticles; i++)
            {
                var startIdx = batch.AddParticleSlot(surfaceID, dims);

                if (i == 0)
                {
                    //Console.WriteLine($"DrawIdx: {startIdx}");
                    emitter.RenderBatch = batch;
                    emitter.BatchIdx = startIdx;
                }
            }
        }

        public void OnCompleted()
        {
            foreach (var drawCall in DrawCalls.Values)
                drawCall.OnCompleted();

            //DebugTextures();
        }

        public void UpdateBuffers()
        {
            foreach (var drawCall in DrawCalls.Values)
                drawCall.UpdateInstanceBuffer();
        }

        public void Draw()
        {
            GraphicsDevice.Indices = Billboard.IndexBuffer;

            foreach (var drawCall in DrawCalls.Values)
                drawCall.Draw();
        }

        public void Dispose()
        {
            foreach (var batch in DrawCalls.Values)
                batch.Dispose();
        }

        public void DebugTextures()
        {
            Console.WriteLine($"DrawCalls: {DrawCalls.Count:N0}");

            foreach (var kvp in DrawCalls)
            {
                var textureFormat = kvp.Key;
                var drawCall = kvp.Value;

                Console.WriteLine();
                Console.WriteLine($"TextureFormat: {textureFormat}");

                foreach (var texture in drawCall.TextureIndex.Keys)
                    Console.WriteLine($" - {texture:X8}");
            }
        }
    }
}
