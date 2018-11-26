﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatExplorer.Entity
{
    public class TexMerge
    {
        public ACE.DatLoader.Entity.TexMerge _texMerge;

        public TexMerge(ACE.DatLoader.Entity.TexMerge texMerge)
        {
            _texMerge = texMerge;
        }

        public List<TreeNode> BuildTree()
        {
            var baseTexSize = new TreeNode($"BaseTextureSize: {_texMerge.BaseTexSize}");

            var cornerTerrainMaps = new TreeNode("CornerTerrainMap:");
            foreach (var cornerTerrainMap in _texMerge.CornerTerrainMaps)
                cornerTerrainMaps.Items.Add(new TreeNode(new TerrainAlphaMap(cornerTerrainMap).ToString()));

            var sideTerrainMaps = new TreeNode("SideTerrainMap:");
            foreach (var sideTerrainMap in _texMerge.SideTerrainMaps)
                sideTerrainMaps.Items.Add(new TreeNode(new TerrainAlphaMap(sideTerrainMap).ToString()));

            // roadalphamap
            var roadAlphaMaps = new TreeNode("RoadAlphaMap:");
            foreach (var roadAlphaMap in _texMerge.RoadMaps)
                roadAlphaMaps.Items.Add(new TreeNode(new RoadAlphaMap(roadAlphaMap).ToString()));

            var terrainDescs = new TreeNode("TMTerrainDesc:");
            for (var i = 0; i < _texMerge.TerrainDesc.Count; i++)
            {
                var terrainDescTree = new TMTerrainDesc(_texMerge.TerrainDesc[i]).BuildTree();

                var terrainDesc = new TreeNode($"{terrainDescTree[0].Name.Replace("TerrainType: ", "")}");
                terrainDesc.Items.AddRange(terrainDescTree[1].Items);

                terrainDescs.Items.Add(terrainDesc);
            }

            return new List<TreeNode>() { baseTexSize, cornerTerrainMaps, sideTerrainMaps, roadAlphaMaps, terrainDescs };
        }
    }
}
