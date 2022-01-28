using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

namespace ACViewer.Render
{
    public class TerrainBatch
    {
        public static Effect Effect => Render.Effect_Clamp;

        public TextureAtlas OverlayAtlas { get; set; }

        public TextureAtlas AlphaAtlas { get; set; }

        public List<TerrainBatchDraw> Batches { get; set; }

        public TerrainBatchDraw CurrentBatch { get; set; }

        public TerrainBatch(TextureAtlas overlayAtlas, TextureAtlas alphaAtlas)
        {
            OverlayAtlas = overlayAtlas;

            AlphaAtlas = alphaAtlas;

            Batches = new List<TerrainBatchDraw>();
        }

        public void AddTerrain(R_Landblock landblock)
        {
            if (CurrentBatch == null || !CurrentBatch.CanAdd(landblock))
            {
                CurrentBatch = new TerrainBatchDraw(OverlayAtlas, AlphaAtlas);
                Batches.Add(CurrentBatch);
            }
            CurrentBatch.AddTerrain(landblock);
        }

        public void OnCompleted()
        {
            foreach (var batch in Batches)
                batch.OnCompleted();
        }

        public void Draw()
        {
            Effect.CurrentTechnique = Effect.Techniques["LandscapeSinglePass"];

            Effect.Parameters["xOverlays"].SetValue(OverlayAtlas._Textures);
            Effect.Parameters["xAlphas"].SetValue(AlphaAtlas._Textures);

            foreach (var batch in Batches)
                batch.Draw();
        }

        public void Dispose()
        {
            foreach (var batch in Batches)
                batch.Dispose();
        }
    }
}
