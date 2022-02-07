using System.Collections.Generic;

namespace ACViewer.Model
{
    public class PartChange
    {
        public uint NewGfxObjId { get; set; }
        public Dictionary<uint, uint> TextureChanges { get; set; }

        public PartChange(uint newGfxObjId)
        {
            NewGfxObjId = newGfxObjId;
        }

        public void AddTexture(uint oldTexture, uint newTexture)
        {
            if (TextureChanges == null)
                TextureChanges = new Dictionary<uint, uint>();

            //TextureChanges.Add(oldTexture, newTexture);
            TextureChanges[oldTexture] = newTexture;
        }
    }
}
