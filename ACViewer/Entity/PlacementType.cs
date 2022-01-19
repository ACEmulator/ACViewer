using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class PlacementType
    {
        public ACE.DatLoader.Entity.PlacementType _placementType;

        public PlacementType(ACE.DatLoader.Entity.PlacementType placementType)
        {
            _placementType = placementType;
        }

        public List<TreeNode> BuildTree()
        {
            return new AnimationFrame(_placementType.AnimFrame).BuildTree();
        }
    }
}
