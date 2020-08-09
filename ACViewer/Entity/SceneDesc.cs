using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class SceneDesc
    {
        public ACE.DatLoader.Entity.SceneDesc _sceneDesc;

        public SceneDesc(ACE.DatLoader.Entity.SceneDesc sceneDesc)
        {
            _sceneDesc = sceneDesc;
        }

        public List<TreeNode> BuildTree()
        {
            var sceneTypes = new TreeNode("SceneTypes:");

            for (var i = 0; i < _sceneDesc.SceneTypes.Count; i++)
            {
                var sceneType = new TreeNode($"{i}");
                sceneType.Items.AddRange(new SceneType(_sceneDesc.SceneTypes[i]).BuildTree());

                sceneTypes.Items.Add(sceneType);
            }
            return new List<TreeNode>() { sceneTypes };
        }
    }
}
