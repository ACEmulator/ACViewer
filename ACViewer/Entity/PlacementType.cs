using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
