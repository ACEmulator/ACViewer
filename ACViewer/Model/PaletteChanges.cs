using System;
using System.Collections.Generic;

using ACE.DatLoader;
using ACE.DatLoader.Entity;
using ACE.DatLoader.FileTypes;

namespace ACViewer.Model
{
    public class PaletteChanges: IEquatable<PaletteChanges>
    {
        public List<CloSubPalette> CloSubPalettes { get; set; }   // from ClothingTable.ClothingSubPalEffects[PaletteTemplate]

        public List<uint> PaletteIds { get; set; }   // for each CloSubPalette.PaletteSet, from PaletteSet.GetPaletteID(shade)

        public PaletteChanges(List<CloSubPalette> subPalettes, float shade = 0.0f)
        {
            CloSubPalettes = subPalettes;

            PaletteIds = GetPaletteIDs(subPalettes, shade);
        }

        public void Add(List<CloSubPalette> subPalettes, float shade = 0.0f)
        {
            var paletteIDs = GetPaletteIDs(subPalettes, shade);

            // the original List<CoSubPalettes> are from DatLoader,
            // so we create a clone here to append data
            CloSubPalettes = new List<CloSubPalette>(CloSubPalettes);   
            CloSubPalettes.AddRange(subPalettes);

            PaletteIds.AddRange(paletteIDs);
        }

        public List<uint> GetPaletteIDs(List<CloSubPalette> subPalettes, float shade = 0.0f)
        {
            var paletteIDs = new List<uint>();
            
            foreach (var subpalette in subPalettes)
            {
                if (subpalette.PaletteSet >> 24 == 0xF)
                {
                    var paletteSet = DatManager.PortalDat.ReadFromDat<PaletteSet>(subpalette.PaletteSet);
                    var paletteId = paletteSet.GetPaletteID(shade);

                    paletteIDs.Add(paletteId);
                }
                else
                    paletteIDs.Add(subpalette.PaletteSet);
            }
            return paletteIDs;
        }

        public bool Equals(PaletteChanges paletteChanges)
        {
            if (CloSubPalettes.Count != paletteChanges.CloSubPalettes.Count)
                return false;

            for (var i = 0; i < CloSubPalettes.Count; i++)
            {
                var a = CloSubPalettes[i];
                var b = paletteChanges.CloSubPalettes[i];

                if (a.PaletteSet != b.PaletteSet)
                    return false;

                if (a.Ranges.Count != b.Ranges.Count)
                    return false;

                for (var j = 0; j < a.Ranges.Count; j++)
                {
                    var ar = a.Ranges[j];
                    var br = b.Ranges[j];

                    if (ar.NumColors != br.NumColors || ar.Offset != br.Offset)
                        return false;
                }
            }

            if (PaletteIds.Count != paletteChanges.PaletteIds.Count)
                return false;

            for (var i = 0; i < PaletteIds.Count; i++)
            {
                if (PaletteIds[i] != paletteChanges.PaletteIds[i])
                    return false;
            }
            
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 0;

            foreach (var cloSubPalette in CloSubPalettes)
            {
                hash = (hash * 397) ^ cloSubPalette.PaletteSet.GetHashCode();

                foreach (var range in cloSubPalette.Ranges)
                {
                    hash = (hash * 397) ^ range.Offset.GetHashCode();
                    hash = (hash * 397) ^ range.NumColors.GetHashCode();
                }
            }

            foreach (var paletteID in PaletteIds)
                hash = (hash * 397) ^ paletteID.GetHashCode();

            return hash;
        }
    }
}
