using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class Generator
    {
        public ACE.DatLoader.Entity.Generator _generator;

        public Generator(ACE.DatLoader.Entity.Generator generator)
        {
            _generator = generator;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            /*if (_generator.Id != 0)
                treeNode.Add(new TreeNode($"Id: {_generator.Id}"));
            
            if (!string.IsNullOrEmpty(_generator.Name))
                treeNode.Add(new TreeNode($"Name: {_generator.Name}"));*/
            
            if (_generator.Items.Count > 0)
            {
                //var items = new TreeNode($"Items");

                foreach (var item in _generator.Items)
                {
                    var heading = item.Id != 0 ? $"{item.Id} - {item.Name}" : item.Name;

                    var subGenerator = new TreeNode(heading);
                    subGenerator.Items = new Generator(item).BuildTree();

                    //items.Items.Add(subGenerator);
                    treeNode.Add(subGenerator);
                }
                //treeNode.Add(items);
            }
            return treeNode;
        }
    }
}
