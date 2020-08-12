using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class AnimPartChange
    {
        public ACE.DatLoader.Entity.AnimationPartChange _animChange;

        public AnimPartChange(ACE.DatLoader.Entity.AnimationPartChange animChange)
        {
            _animChange = animChange;
        }

        public List<TreeNode> BuildTree()
        {
            var partIdx = new TreeNode($"PartIdx: {_animChange.PartIndex}");
            var partID = new TreeNode($"PartID: {_animChange.PartID:X8}");

            return new List<TreeNode>() { partIdx, partID };
        }

        public override string ToString()
        {
            return $"PartIdx: {_animChange.PartIndex}, PartID: {_animChange.PartID:X8}";
        }
    }
}
