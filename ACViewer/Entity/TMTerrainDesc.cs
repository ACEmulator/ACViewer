using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class TMTerrainDesc
    {
        public ACE.DatLoader.Entity.TMTerrainDesc _tmTerrainDesc;

        public TMTerrainDesc(ACE.DatLoader.Entity.TMTerrainDesc tmTerrainDesc)
        {
            _tmTerrainDesc = tmTerrainDesc;
        }

        public List<TreeNode> BuildTree()
        {
            var terrainType = new TreeNode($"TerrainType: {_tmTerrainDesc.TerrainType}");

            var terrainTex = new TreeNode("TerrainTexture:");
            terrainTex.Items.AddRange(new TerrainTex(_tmTerrainDesc.TerrainTex).BuildTree());

            return new List<TreeNode>() { terrainType, terrainTex };
        }
    }
}
