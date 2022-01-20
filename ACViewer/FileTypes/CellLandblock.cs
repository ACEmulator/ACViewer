using System.Collections.Generic;

using ACE.DatLoader;

using ACViewer.Entity;

using Landblock = ACE.DatLoader.FileTypes.CellLandblock;

namespace ACViewer.FileTypes
{
    public class CellLandblock
    {
        public Landblock _landblock;
        
        public CellLandblock(Landblock landblock)
        {
            _landblock = landblock;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_landblock.Id:X8}");

            var hasObjects = new TreeNode($"HasObjects: {_landblock.HasObjects}");
            var terrain = new TreeNode("Terrain:");
            for (var i = 0; i < _landblock.Terrain.Count; i++)
            {
                var t = _landblock.Terrain[i];
                var typename = DatManager.PortalDat.RegionDesc.TerrainInfo.TerrainTypes[Landblock.GetType(t)].TerrainName;
                terrain.Items.Add(new TreeNode($"{i}: Road: {Landblock.GetRoad(t)}, Type: {typename}, Scenery: {Landblock.GetScenery(t)}"));
                
            }

            var heights = new TreeNode("Heights:");
            for (var i = 0; i < _landblock.Height.Count; i++)
                heights.Items.Add(new TreeNode($"{i}: {_landblock.Height[i]}"));

            treeView.Items.AddRange(new List<TreeNode>() { hasObjects, terrain, heights });

            return treeView;
        }
    }
}
