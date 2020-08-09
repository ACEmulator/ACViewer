using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class DegradeInfo
    {
        public ACE.DatLoader.FileTypes.GfxObjDegradeInfo _degrade;

        public DegradeInfo(ACE.DatLoader.FileTypes.GfxObjDegradeInfo degrade)
        {
            _degrade = degrade;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_degrade.Id:X8}");

            foreach (var degrade in _degrade.Degrades)
            {
                var degradeTree = new GfxObjInfo(degrade).BuildTree();
                var degradeNode = new TreeNode($"{degradeTree[0].Name.Replace("ID: ", "")}");
                degradeTree.RemoveAt(0);
                degradeNode.Items.AddRange(degradeTree);
                treeView.Items.Add(degradeNode);
            }
            return treeView;
        }
    }
}
