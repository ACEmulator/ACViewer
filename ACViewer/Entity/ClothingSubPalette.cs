using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class ClothingSubPalette
    {
        public ACE.DatLoader.Entity.CloSubPalette _subPalette;

        public ClothingSubPalette(ACE.DatLoader.Entity.CloSubPalette subPalette)
        {
            _subPalette = subPalette;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_subPalette.Ranges.Count == 1)
            {
                var range = new TreeNode($"Range: {new ClothingSubPaletteRange(_subPalette.Ranges[0]).ToString()}");
                treeNode.Add(range);
            }
            else
            {
                var ranges = new TreeNode("SubPalette Ranges:");
                foreach (var range in _subPalette.Ranges)
                    ranges.Items.Add(new TreeNode(new ClothingSubPaletteRange(range).ToString()));

                treeNode.Add(ranges);
            }

            var paletteSet = new TreeNode($"Palette Set: {_subPalette.PaletteSet:X8}");
            treeNode.Add(paletteSet);

            return treeNode;
        }
    }
}
