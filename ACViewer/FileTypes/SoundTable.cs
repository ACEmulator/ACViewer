using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACViewer.Entity;
using ACE.Entity.Enum;

namespace ACViewer.FileTypes
{
    public class SoundTable
    {
        public ACE.DatLoader.FileTypes.SoundTable _soundTable;

        public SoundTable(ACE.DatLoader.FileTypes.SoundTable soundTable)
        {
            _soundTable = soundTable;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_soundTable.Id:X8}");

            var hashTable = new TreeNode("SoundHash:");
            foreach (var hash in _soundTable.SoundHash)
            {
                var hashTree = new SoundTableData(hash).BuildTree();
                var hashNode = new TreeNode(hashTree[0].Name.Replace("Sound ID: ", ""));
                hashTree.RemoveAt(0);
                hashNode.Items.AddRange(hashTree);

                hashTable.Items.Add(hashNode);
            }

            var sounds = new TreeNode("Sounds:");
            foreach (var kvp in _soundTable.Data)
            {
                var sound = new TreeNode($"{(ACE.Entity.Enum.Sound)kvp.Key}");
                sound.Items.AddRange(new SoundData(kvp.Value).BuildTree());
                sounds.Items.Add(sound);
            }

            treeView.Items.AddRange(new List<TreeNode>() { hashTable, sounds });

            return treeView;
        }
    }
}
