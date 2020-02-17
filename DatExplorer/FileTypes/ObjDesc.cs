using ACE.DatLoader.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatExplorer.FileTypes
{

    /// <summary>
    /// Used in the Clothing Table entries to change the look/color of a default model (e.g. because it has clothing equipped)
    /// </summary>
    public class ObjDesc
    {
        public uint PaletteID { get; private set; }
        public List<SubPalette> SubPalettes { get; } = new List<SubPalette>();
        public List<TextureMapChange> TextureChanges { get; } = new List<TextureMapChange>();
        public List<AnimationPartChange> AnimPartChanges { get; } = new List<AnimationPartChange>();

    }
}
