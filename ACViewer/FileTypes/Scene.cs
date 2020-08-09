using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class Scene
    {
        public ACE.DatLoader.FileTypes.Scene _scene;

        public Scene(ACE.DatLoader.FileTypes.Scene scene)
        {
            _scene = scene;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_scene.Id:X8}");

            foreach (var objDesc in _scene.Objects)
            {
                var desc = new ObjectDesc(objDesc).BuildTree();
                var obj = new TreeNode(desc[0].Name.Replace("Object ID: ", ""));
                desc.RemoveAt(0);
                obj.Items.AddRange(desc);
                treeView.Items.Add(obj);
            }
            return treeView;
        }
    }
}
