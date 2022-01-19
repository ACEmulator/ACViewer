using System.Collections.Generic;

using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class GeneratorTable
    {
        public ACE.DatLoader.FileTypes.GeneratorTable _generatorTable;

        public GeneratorTable(ACE.DatLoader.FileTypes.GeneratorTable generatorTable)
        {
            _generatorTable = generatorTable;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_generatorTable.Id:X8}");

            var generators = new TreeNode($"Generators");
            generators.Items = new Generator(_generatorTable.Generators).BuildTree();

            var playDayItems = new TreeNode($"PlayDayItems");
            playDayItems.Items = BuildGenerators(_generatorTable.PlayDayItems);

            var weenieObjectsItems = new TreeNode($"WeenieObjectsItems");
            weenieObjectsItems.Items = BuildGenerators(_generatorTable.WeenieObjectsItems);

            treeView.Items = new List<TreeNode>() { generators, playDayItems, weenieObjectsItems };

            return treeView;
        }

        public List<TreeNode> BuildGenerators(List<ACE.DatLoader.Entity.Generator> generators)
        {
            var nodes = new List<TreeNode>();
            
            foreach (var item in generators)
            {
                var heading = item.Id != 0 ? $"{item.Id} - {item.Name}" : item.Name;

                var node = new TreeNode(heading);
                node.Items = new Generator(item).BuildTree();

                nodes.Add(node);
            }
            return nodes;
        }
    }
}
