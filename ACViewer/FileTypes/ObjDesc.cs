using System.Collections.Generic;

using ACE.DatLoader.Entity;

namespace ACViewer.FileTypes
{
    /// <summary>
    /// Used in the Clothing Table entries to change the look/color of a default model (e.g. because it has clothing equipped)
    /// </summary>
    public class ObjDesc
    {
        public Dictionary<byte, List<TextureMapChange>> TextureChanges { get; } = new Dictionary<byte, List<TextureMapChange>>();
        public Dictionary<byte, AnimationPartChange> AnimPartChanges { get; } = new Dictionary<byte, AnimationPartChange>();
    }
}
