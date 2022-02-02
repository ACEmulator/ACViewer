using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

namespace ACViewer.Render
{
    public class TerrainBatch
    {
        public static Effect Effect => Render.Effect_Clamp;

        public TextureAtlasChain OverlayAtlasChain { get; set; }

        public TextureAtlasChain AlphaAtlasChain { get; set; }

        public List<TerrainBatchDraw> Batches { get; set; }

        public TerrainBatchDraw CurrentBatch { get; set; }

        public TerrainBatch(TextureAtlasChain overlayAtlasChain, TextureAtlasChain alphaAtlasChain)
        {
            OverlayAtlasChain = overlayAtlasChain;

            AlphaAtlasChain = alphaAtlasChain;

            Batches = new List<TerrainBatchDraw>();
        }

        public void AddTerrain(R_Landblock landblock)
        {
            if (CurrentBatch == null || !CurrentBatch.CanAdd(landblock))
            {
                CurrentBatch = new TerrainBatchDraw(OverlayAtlasChain, AlphaAtlasChain);
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

            // assumed to be at index 0
            if (OverlayAtlasChain.TextureAtlases.Count > 0)
                Effect.Parameters["xOverlays"].SetValue(OverlayAtlasChain.TextureAtlases[0]._Textures);

            if (AlphaAtlasChain.TextureAtlases.Count > 0)
                Effect.Parameters["xAlphas"].SetValue(AlphaAtlasChain.TextureAtlases[0]._Textures);

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
