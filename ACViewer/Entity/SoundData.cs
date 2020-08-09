using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class SoundData
    {
        public ACE.DatLoader.Entity.SoundData _soundData;

        public SoundData(ACE.DatLoader.Entity.SoundData soundData)
        {
            _soundData = soundData;
        }

        public List<TreeNode> BuildTree()
        {
            var soundTable = new TreeNode("SoundTable:");

            foreach (var sound in _soundData.Data)
            {
                var soundTree = new SoundTableData(sound).BuildTree();
                var soundNode = new TreeNode(soundTree[0].Name.Replace("Sound ID: ", ""));
                soundTree.RemoveAt(0);
                soundNode.Items.AddRange(soundTree);
                soundTable.Items.Add(soundNode);
            }
            var unknown = new TreeNode($"Unknown: {_soundData.Unknown}");

            return new List<TreeNode>() { soundTable, unknown };
        }
    }
}
