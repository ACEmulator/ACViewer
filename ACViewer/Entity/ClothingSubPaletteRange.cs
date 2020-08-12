using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class ClothingSubPaletteRange
    {
        public ACE.DatLoader.Entity.CloSubPaletteRange _range;

        public ClothingSubPaletteRange(ACE.DatLoader.Entity.CloSubPaletteRange range)
        {
            _range = range;
        }

        public List<TreeNode> BuildTree()
        {
            var offset = new TreeNode($"Offset: {_range.Offset}");
            var numColors = new TreeNode($"NumColors: {_range.NumColors}");

            return new List<TreeNode>() { offset, numColors };
        }

        public override string ToString()
        {
            return $"Offset: {_range.Offset}, NumColors: {_range.NumColors}";
        }
    }
}
