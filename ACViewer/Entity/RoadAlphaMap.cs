using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class RoadAlphaMap
    {
        public ACE.DatLoader.Entity.RoadAlphaMap _roadAlphaMap;

        public RoadAlphaMap(ACE.DatLoader.Entity.RoadAlphaMap roadAlphaMap)
        {
            _roadAlphaMap = roadAlphaMap;
        }

        public List<TreeNode> BuildTree()
        {
            var roadCode = new TreeNode($"RoadCode: {_roadAlphaMap.RCode}");
            var roadTexGID = new TreeNode($"RoadTexGID: {_roadAlphaMap.RoadTexGID:X8}");

            return new List<TreeNode>() { roadCode, roadTexGID };
        }

        public override string ToString()
        {
            return $"RoadCode: {_roadAlphaMap.RCode}, RoadTexGID: {_roadAlphaMap.RoadTexGID:X8}";
        }
    }
}
