using System;

using ACViewer.Model;

namespace ACViewer.Render
{
    public class TextureChanges: IEquatable<TextureChanges>
    {
        public uint TextureId { get; set; }   // 0x6 texture file id
        public float Translucency { get; set; }
        public PaletteChanges PaletteChanges { get; set; } 

        public TextureChanges(uint textureId, float translucency = 0.0f, PaletteChanges paletteChanges = null)
        {
            TextureId = textureId;
            Translucency = translucency;
            PaletteChanges = paletteChanges;
        }

        public bool Equals(TextureChanges tp)
        {
            if (TextureId != tp.TextureId)
                return false;

            if (Translucency != tp.Translucency)
                return false;

            if (PaletteChanges == null && tp.PaletteChanges == null)
                return true;

            if (PaletteChanges == null && tp.PaletteChanges != null || PaletteChanges != null && tp.PaletteChanges == null)
                return false;

            if (!PaletteChanges.Equals(tp.PaletteChanges))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            int hash = 0;

            hash = (hash * 397) ^ TextureId.GetHashCode();
            
            hash = (hash * 397) ^ Translucency.GetHashCode();

            if (PaletteChanges != null)
                hash = (hash * 397) ^ PaletteChanges.GetHashCode();

            return hash;
        }
    }
}
