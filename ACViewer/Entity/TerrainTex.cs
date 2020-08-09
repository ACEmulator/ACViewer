using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class TerrainTex
    {
        public ACE.DatLoader.Entity.TerrainTex _terrainTex;

        public TerrainTex(ACE.DatLoader.Entity.TerrainTex terrainTex)
        {
            _terrainTex = terrainTex;
        }

        public List<TreeNode> BuildTree()
        {
            var texGID = new TreeNode($"TexGID: {_terrainTex.TexGID:X8}");
            var texTiling = new TreeNode($"TexTiling: {_terrainTex.TexTiling}");
            var maxVertBright = new TreeNode($"MaxVertBrightness: {_terrainTex.MaxVertBright}");
            var minVertBright = new TreeNode($"MinVertBrightness: {_terrainTex.MinVertBright}");
            var maxVertSaturate = new TreeNode($"MaxVertSaturate: {_terrainTex.MaxVertSaturate}");
            var minVertSaturate = new TreeNode($"MinVertSaturate: {_terrainTex.MinVertSaturate}");
            var maxVertHue = new TreeNode($"MaxVertHue: {_terrainTex.MaxVertHue}");
            var minVertHue = new TreeNode($"MinVertHue: {_terrainTex.MinVertHue}");
            var detailTexTiling = new TreeNode($"DetailTexTiling: {_terrainTex.DetailTexTiling}");
            var detailTexGID = new TreeNode($"DetailTexGID: {_terrainTex.DetailTexGID:X8}");

            return new List<TreeNode>() { texGID, texTiling, maxVertBright, minVertBright, maxVertSaturate, minVertSaturate, maxVertHue, minVertHue, detailTexTiling, detailTexGID };
        }
    }
}
