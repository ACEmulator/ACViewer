using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class CharGen
    {
        public ACE.DatLoader.FileTypes.CharGen _charGen;

        public CharGen(ACE.DatLoader.FileTypes.CharGen charGen)
        {
            _charGen = charGen;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_charGen.Id:X8}");

            var starterAreas = new TreeNode("Starter Areas");
            foreach (var starterArea in _charGen.StarterAreas)
            {
                var starterAreaTree = new StarterArea(starterArea).BuildTree();
                var starterAreaNode = new TreeNode(starterAreaTree[0].Name.Replace("Name: ", ""));
                starterAreaNode.Items.AddRange(starterAreaTree[1].Items);
                starterAreas.Items.Add(starterAreaNode);
            }

            var heritageGroups = new TreeNode("Heritage Groups");
            foreach (var kvp in _charGen.HeritageGroups)
            {
                var heritageTree = new HeritageGroupCG(kvp.Value).BuildTree();
                var hg = new TreeNode(heritageTree[0].Name.Replace("Name: ", ""));
                heritageTree.RemoveAt(0);
                hg.Items.AddRange(heritageTree);
                heritageGroups.Items.Add(hg);
            }

            treeView.Items.AddRange(new List<TreeNode>() { starterAreas, heritageGroups });

            return treeView;
        }
    }
}
