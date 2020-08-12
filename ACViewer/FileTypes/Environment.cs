using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class Environment
    {
        public ACE.DatLoader.FileTypes.Environment _env;

        public Environment(ACE.DatLoader.FileTypes.Environment env)
        {
            _env = env;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_env.Id:X8}");

            foreach (var kvp in _env.Cells)
            {
                var cellStruct = new TreeNode($"{kvp.Key}");
                cellStruct.Items.AddRange(new CellStruct(kvp.Value).BuildTree());
                treeView.Items.Add(cellStruct);
            }
            return treeView;
        }
    }
}
