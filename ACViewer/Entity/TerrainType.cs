using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class TerrainType
    {
        public ACE.DatLoader.Entity.TerrainType _terrainType;

        public TerrainType(ACE.DatLoader.Entity.TerrainType terrainType)
        {
            _terrainType = terrainType;
        }

        public List<TreeNode> BuildTree()
        {
            var terrainName = new TreeNode($"TerrainName: {_terrainType.TerrainName}");
            var terrainColor = new TreeNode($"TerrainColor: {_terrainType.TerrainColor:X8}");

            var sceneTypes = new TreeNode("SceneTypes:");
            for (var i = 0; i < _terrainType.SceneTypes.Count; i++)
                sceneTypes.Items.Add(new TreeNode($"{i}: {_terrainType.SceneTypes[i]}"));

            return new List<TreeNode>() { terrainName, terrainColor, sceneTypes };
        }
    }
}
