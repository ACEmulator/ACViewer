using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class TreeNode
    {
        public string Name { get; set; }
        public List<TreeNode> Items { get; set; }

        public TreeNode(string name = "", List<TreeNode> items = null)
        {
            Name = name;
            Items = items ?? new List<TreeNode>();
        }
    }
}
