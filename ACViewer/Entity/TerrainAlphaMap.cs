using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class TerrainAlphaMap
    {
        public ACE.DatLoader.Entity.TerrainAlphaMap _terrainAlphaMap;

        public TerrainAlphaMap(ACE.DatLoader.Entity.TerrainAlphaMap terrainAlphaMap)
        {
            _terrainAlphaMap = terrainAlphaMap;
        }

        public List<TreeNode> BuildTree()
        {
            var terrainCode = new TreeNode($"TerrainCode: {_terrainAlphaMap.TCode}");
            var textureGID = new TreeNode($"TextureGID: {_terrainAlphaMap.TexGID:X8}");

            return new List<TreeNode>() { terrainCode, textureGID };
        }

        public override string ToString()
        {
            return $"TerrainCode: {_terrainAlphaMap.TCode}, TextureGID: {_terrainAlphaMap.TexGID:X8}";
        }
    }
}
