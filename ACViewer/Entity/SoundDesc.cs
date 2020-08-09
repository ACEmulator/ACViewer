using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class SoundDesc
    {
        public ACE.DatLoader.Entity.SoundDesc _soundDesc;

        public SoundDesc(ACE.DatLoader.Entity.SoundDesc soundDesc)
        {
            _soundDesc = soundDesc;
        }

        public List<TreeNode> BuildTree()
        {
            var soundInfos = new List<TreeNode>();

            for (var i = 0; i < _soundDesc.STBDesc.Count; i++)
            {
                var tree = new AmbientSoundTableDesc(_soundDesc.STBDesc[i]).BuildTree();
                var node = new TreeNode($"{i} - {tree[0].Name.Replace("Ambient Sound Table ID: ", "")}");
                tree.RemoveAt(0);
                node.Items.AddRange(tree);

                soundInfos.Add(node);
            }
            return soundInfos;
        }
    }
}
