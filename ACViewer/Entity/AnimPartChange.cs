using System.Collections.Generic;

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
            var partID = new TreeNode($"PartID: {_animChange.PartID:X8}", clickable: true);

            return new List<TreeNode>() { partIdx, partID };
        }

        public override string ToString()
        {
            return $"PartIdx: {_animChange.PartIndex}, PartID: {_animChange.PartID:X8}";
        }
    }
}
