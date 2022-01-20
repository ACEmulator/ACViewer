using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class SceneType
    {
        public ACE.DatLoader.Entity.SceneType _sceneType;

        public SceneType(ACE.DatLoader.Entity.SceneType sceneType)
        {
            _sceneType = sceneType;
        }

        public List<TreeNode> BuildTree()
        {
            var sceneTableIdx = new TreeNode($"SceneTableIdx: {_sceneType.StbIndex}");

            var scenes = new TreeNode($"Scenes:");

            foreach (var scene in _sceneType.Scenes)
                scenes.Items.Add(new TreeNode($"{scene:X8}", clickable: true));

            return new List<TreeNode>() { sceneTableIdx, scenes };
        }
    }
}
