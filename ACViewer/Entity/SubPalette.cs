using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class SubPalette
    {
        public ACE.DatLoader.Entity.SubPalette _subPalette;

        public SubPalette(ACE.DatLoader.Entity.SubPalette subPalette)
        {
            _subPalette = subPalette;
        }

        public List<TreeNode> BuildTree()
        {
            var subID = new TreeNode($"SubID: {_subPalette.SubID:X8}");
            var offset = new TreeNode($"Offset: {_subPalette.Offset:X8}");
            var numColors = new TreeNode($"NumColors: {_subPalette.NumColors}");

            return new List<TreeNode>() { subID, offset, numColors };
        }
    }
}
