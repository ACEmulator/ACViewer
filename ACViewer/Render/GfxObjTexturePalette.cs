using System;
using System.Collections.Generic;

using ACViewer.Model;

namespace ACViewer.Render
{
    public class GfxObjTexturePalette : IEquatable<GfxObjTexturePalette>
    {
        public uint GfxObjId { get; set; }
        public Dictionary<uint, uint> TextureChanges { get; set; }
        public PaletteChanges PaletteChanges { get; set; }

        public GfxObjTexturePalette(uint gfxObjId, Dictionary<uint, uint> textureChanges = null, PaletteChanges paletteChanges = null)
        {
            GfxObjId = gfxObjId;
            TextureChanges = textureChanges;
            PaletteChanges = paletteChanges;
        }

        public bool Equals(GfxObjTexturePalette tpc)
        {
            if (GfxObjId != tpc.GfxObjId) return false;
            
            if (TextureChanges == null && tpc.TextureChanges != null || TextureChanges != null && tpc.TextureChanges == null)
                return false;

            if (TextureChanges != null && tpc.TextureChanges != null)
            {
                foreach (var kvp in TextureChanges)
                {
                    if (!tpc.TextureChanges.TryGetValue(kvp.Key, out var texChange) || kvp.Value != texChange)
                        return false;
                }
            }

            if (PaletteChanges == null && tpc.PaletteChanges != null || PaletteChanges != null && tpc.PaletteChanges == null)
                return false;

            if (PaletteChanges != null && tpc.PaletteChanges != null)
            {
                if (!PaletteChanges.Equals(tpc.PaletteChanges))
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 0;

            hash = (hash * 397) ^ GfxObjId.GetHashCode();
            
            if (TextureChanges != null)
            {
                foreach (var kvp in TextureChanges)
                {
                    hash = (hash * 397) ^ kvp.Key.GetHashCode();
                    hash = (hash * 397) ^ kvp.Value.GetHashCode();
                }
            }

            if (PaletteChanges != null)
                hash = (hash * 397) ^ PaletteChanges.GetHashCode();

            return hash;
        }
    }
}
