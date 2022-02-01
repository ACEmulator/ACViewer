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
    }
}
