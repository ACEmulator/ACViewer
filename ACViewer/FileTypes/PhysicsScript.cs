using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class PhysicsScript
    {
        public ACE.DatLoader.FileTypes.PhysicsScript _playScript;

        public PhysicsScript(ACE.DatLoader.FileTypes.PhysicsScript playScript)
        {
            _playScript = playScript;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_playScript.Id:X8}");

            var scripts = new TreeNode("Scripts:");

            for (var i = 0; i < _playScript.ScriptData.Count; i++)
            {
                var script = new TreeNode($"{i}");
                script.Items.AddRange(new PhysicsScriptData(_playScript.ScriptData[i]).BuildTree());

                scripts.Items.Add(script);
            }
            treeView.Items.AddRange(scripts.Items);
            return treeView;
        }
    }
}
