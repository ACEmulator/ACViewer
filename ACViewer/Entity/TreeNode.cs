using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
