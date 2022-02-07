using System.Collections.Generic;
using System.Linq;
using System.Text;

using ACE.DatLoader;
using ACE.DatLoader.Entity;
using ACE.DatLoader.FileTypes;
using ACE.Entity.Enum;
using ACE.Server.WorldObjects;

namespace ACViewer.Model
{
    /// <summary>
    /// Used in the Clothing Table entries to change the look/color of a default model (e.g. because it has clothing equipped)
    /// </summary>
    public class ObjDesc
    {
        public uint SetupId { get; set; }
        public uint? PaletteBaseDID { get; set; }
        public PaletteChanges PaletteChanges { get; set; }
        public Dictionary<uint, PartChange> PartChanges { get; set; }

        public ObjDesc(uint setupID, uint clothingTableID, PaletteTemplate paletteTemplate = PaletteTemplate.Undef, float shade = 0.0f)
        {
            SetupId = setupID;
            
            Add(clothingTableID, paletteTemplate, shade);
        }

        public void Add(uint clothingTableID, PaletteTemplate paletteTemplate = PaletteTemplate.Undef, float shade = 0.0f)
        {
            var clothingTable = DatManager.PortalDat.ReadFromDat<ClothingTable>(clothingTableID);

            if (!clothingTable.ClothingBaseEffects.TryGetValue(SetupId, out var baseEffect)) return;

            // palette changes
            if (clothingTable.ClothingSubPalEffects.TryGetValue((uint)paletteTemplate, out var palEffect))
            {
                if (PaletteChanges == null)
                    PaletteChanges = new PaletteChanges(palEffect.CloSubPalettes, shade);
                else
                    PaletteChanges.Add(palEffect.CloSubPalettes, shade);
            }

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

        public void AddBaseModelData(WorldObject wo)
        {
            // Hair/head
            var headChange = GetPartChange(16);

            // This Heritage check has been added for backwards compatibility. It works around the butthead Gear Knights appearance.
            if (headChange != null && wo.HeadObjectDID != null && wo.HairStyle == null & wo.Heritage != null && wo.Heritage != (int)HeritageGroup.Gearknight)
            {
                headChange.NewGfxObjId = wo.HeadObjectDID.Value;
            }
            else if (wo.HairStyle != null && wo.Heritage != null && wo.Gender != null)
            {
                // This indicates we have a Gear Knight or Olthoi (that is, player types treat "hairstyle" as "body style")

                // Load the CharGen data. It has all the gfxobj & texture changes for the body style defined within it
                var cg = DatManager.PortalDat.CharGen;

                var sex = cg.HeritageGroups[(uint)wo.Heritage].Genders[(int)wo.Gender];

                if ((int)wo.HairStyle < sex.HairStyleList.Count)
                {
                    var hairstyle = sex.HairStyleList[(int)wo.HairStyle];

                    // add gfxobj changes
                    foreach (var part in hairstyle.ObjDesc.AnimPartChanges)
                        GetPartChange(part.PartIndex, part.PartID);

                    // add texture changes
                    foreach (var textureChange in hairstyle.ObjDesc.TextureChanges)
                    {
                        var partChange = GetPartChange(textureChange.PartIndex);

                        partChange.AddTexture(textureChange.OldTexture, textureChange.NewTexture);
                    }
                }
            }

            if (wo is ACE.Server.WorldObjects.Player player)
                headChange.AddTexture(player.Character.DefaultHairTexture, player.Character.HairTexture);

            if (wo.HairPaletteDID != null)
                AddPaletteChange(wo.HairPaletteDID.Value, 24, 8);

            // Skin
            //PaletteBaseDID = wo.PaletteBaseDID;     // bleh, figure out how to handle this

            if (wo.SkinPaletteDID != null)
                AddPaletteChange(wo.SkinPaletteDID.Value, 0, 24);

            // Eyes
            if (wo.DefaultEyesTextureDID != null && wo.EyesTextureDID != null)
            {
                headChange.AddTexture(wo.DefaultEyesTextureDID.Value, wo.EyesTextureDID.Value);
            }

            if (wo.EyesPaletteDID != null)
                AddPaletteChange(wo.EyesPaletteDID.Value, 32, 8);

            // Nose
            if (wo.DefaultNoseTextureDID != null && wo.NoseTextureDID != null)
                headChange.AddTexture(wo.DefaultNoseTextureDID.Value, wo.NoseTextureDID.Value);

            // Mouth
            if (wo.DefaultMouthTextureDID != null && wo.MouthTextureDID != null)
                headChange.AddTexture(wo.DefaultMouthTextureDID.Value, wo.MouthTextureDID.Value);

            // oh god why
            if (wo.Weenie.PropertiesPalette != null)
            {
                foreach (var palette in wo.Weenie.PropertiesPalette)
                    AddPaletteChange(palette.SubPaletteId, palette.Offset, palette.Length);
            }

            if (wo.Weenie.PropertiesAnimPart != null)
            {
                foreach (var animPart in wo.Weenie.PropertiesAnimPart)
                    GetPartChange(animPart.Index, animPart.AnimationId);
            }

            if (wo.Weenie.PropertiesTextureMap != null)
            {
                foreach (var textureMap in wo.Weenie.PropertiesTextureMap)
                {
                    var partChange = GetPartChange(textureMap.PartIndex);

                    partChange.AddTexture(textureMap.OldTexture, textureMap.NewTexture);
                }
            }
        }

        private PartChange GetPartChange(uint partIdx, uint? newGfxObjId = null)
        {
            if (PartChanges == null)
                PartChanges = new Dictionary<uint, PartChange>();
            
            if (!PartChanges.TryGetValue(partIdx, out var partChange))
            {
                if (newGfxObjId == null)
                {
                    var setup = DatManager.PortalDat.ReadFromDat<SetupModel>(SetupId);

                    if (partIdx >= setup.Parts.Count)
                        return null;

                    newGfxObjId = setup.Parts[(int)partIdx];
                }

                partChange = new PartChange(newGfxObjId.Value);
                PartChanges.Add(partIdx, partChange);
            }
            return partChange;
        }

        private void AddPaletteChange(uint paletteID, uint offset, uint length)
        {
            var cloSubPalette = new CloSubPalette();
            cloSubPalette.PaletteSet = paletteID;

            var cloSubPaletteRange = new CloSubPaletteRange();
            cloSubPaletteRange.Offset = offset * 8;
            cloSubPaletteRange.NumColors = length * 8;

            cloSubPalette.Ranges.Add(cloSubPaletteRange);

            var cloSubPalettes = new List<CloSubPalette>() { cloSubPalette };

            if (PaletteChanges == null)
                PaletteChanges = new PaletteChanges(cloSubPalettes);
            else
                PaletteChanges.Add(cloSubPalettes);

        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"SetupID: {SetupId:X8}");

            if (PaletteChanges != null)
            {
                sb.AppendLine($"PaletteChanges:");
                sb.AppendLine();
                sb.AppendLine($"CloSubPalettes:");
                sb.AppendLine();
                foreach (var cloSubPalette in PaletteChanges.CloSubPalettes)
                {
                    var ranges = new List<string>();

                    foreach (var range in cloSubPalette.Ranges)
                        ranges.Add($"Offset: {range.Offset:N0}, Length: {range.NumColors:N0}");

                    sb.AppendLine($"PaletteSet: {cloSubPalette.PaletteSet:X8}, Ranges: {string.Join(", ", ranges)}");
                }
                sb.AppendLine();
                sb.AppendLine($"PaletteIDs: {string.Join(", ", PaletteChanges.PaletteIds.Select(i => i.ToString("X8")))}");
            }

            return sb.ToString();
        }
    }
}
