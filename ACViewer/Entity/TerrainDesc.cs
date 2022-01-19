using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class TerrainDesc
    {
        public ACE.DatLoader.Entity.TerrainDesc _terrainDesc;

        public TerrainDesc(ACE.DatLoader.Entity.TerrainDesc terrainDesc)
        {
            _terrainDesc = terrainDesc;
        }

        public List<TreeNode> BuildTree()
        {
            var terrainTypes = new TreeNode("TerrainTypes:");
            for (var i = 0; i < _terrainDesc.TerrainTypes.Count; i++)
            {
                var terrainTypeTree = new TerrainType(_terrainDesc.TerrainTypes[i]).BuildTree();

                var terrainType = new TreeNode($"{terrainTypeTree[0].Name.Replace("TerrainName: ", "")}");
                terrainTypeTree.RemoveAt(0);
                terrainType.Items.AddRange(terrainTypeTree);

                terrainTypes.Items.Add(terrainType);
            }

            var landSurf = new TreeNode("LandSurf:");
            landSurf.Items.AddRange(new LandSurf(_terrainDesc.LandSurfaces).BuildTree());

            return new List<TreeNode>() { terrainTypes, landSurf };
        }
    }
}
