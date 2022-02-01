using System.Collections.Generic;

using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using ACE.Entity.Enum;

namespace ACViewer.Model
{
    /// <summary>
    /// Used in the Clothing Table entries to change the look/color of a default model (e.g. because it has clothing equipped)
    /// </summary>
    public class ObjDesc
    {
        public PaletteChanges PaletteChanges { get; set; }
        public Dictionary<uint, PartChange> PartChanges { get; set; }

        public ObjDesc(uint setupID, uint clothingTableID, PaletteTemplate paletteTemplate = PaletteTemplate.Undef, float shade = 0.0f)
        {
            Add(setupID, clothingTableID, paletteTemplate, shade);
        }

        public void Add(uint setupID, uint clothingTableID, PaletteTemplate paletteTemplate = PaletteTemplate.Undef, float shade = 0.0f)
        {
            var clothingTable = DatManager.PortalDat.ReadFromDat<ClothingTable>(clothingTableID);

            // palette changes
            if (clothingTable.ClothingSubPalEffects.TryGetValue((uint)paletteTemplate, out var palEffect))
            {
                if (PaletteChanges == null)
                    PaletteChanges = new PaletteChanges(palEffect.CloSubPalettes, shade);
                else
                    PaletteChanges.Add(palEffect.CloSubPalettes, shade);
            }

            if (!clothingTable.ClothingBaseEffects.TryGetValue(setupID, out var baseEffect)) return;

            foreach (var objEffect in baseEffect.CloObjectEffects)
            {
                if (PartChanges == null)
                    PartChanges = new Dictionary<uint, PartChange>();

                // gfxobj change
                if (!PartChanges.TryGetValue(objEffect.Index, out var partChange))
                {
                    partChange = new PartChange(objEffect.ModelId);
                    PartChanges.Add(objEffect.Index, partChange);
                }
                else
                    partChange.NewGfxObjId = objEffect.ModelId;

                // texture changes
                foreach (var texEffect in objEffect.CloTextureEffects)
                {
                    if (partChange.TextureChanges == null)
                        partChange.TextureChanges = new Dictionary<uint, uint>();

                    partChange.TextureChanges[texEffect.OldTexture] = texEffect.NewTexture;
                }
            }
        }
    }
}
