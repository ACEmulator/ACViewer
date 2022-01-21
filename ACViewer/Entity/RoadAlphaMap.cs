using System.Collections.Generic;

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
            var roadTexGID = new TreeNode($"RoadTexGID: {_roadAlphaMap.RoadTexGID:X8}", clickable: true);

            return new List<TreeNode>() { roadCode, roadTexGID };
        }

        public override string ToString()
        {
            return $"RoadCode: {_roadAlphaMap.RCode}, RoadTexGID: {_roadAlphaMap.RoadTexGID:X8}";
        }
    }
}
