using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class StringTableData
    {
        public ACE.DatLoader.Entity.StringTableData _stringTableData;

        public StringTableData(ACE.DatLoader.Entity.StringTableData stringTableData)
        {
            _stringTableData = stringTableData;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            var id = new TreeNode($"{_stringTableData.Id:X8}");
            treeNode.Add(id);

            if (_stringTableData.VarNames.Count > 0)
            {
                var varNames = new TreeNode("Variable Names:");
                foreach (var varName in _stringTableData.VarNames)
                    varNames.Items.Add(new TreeNode(varName));

                treeNode.Add(varNames);
            }

            if (_stringTableData.Vars.Count > 0)
            {
                var vars = new TreeNode("Variables:");
                foreach (var var in _stringTableData.Vars)
                    vars.Items.Add(new TreeNode(var));

                treeNode.Add(vars);
            }

            if (_stringTableData.Strings.Count > 0)
            {
                var strings = new TreeNode("Strings:");
                foreach (var str in _stringTableData.Strings)
                    strings.Items.Add(new TreeNode(str));

                treeNode.Add(strings);
            }

            if (_stringTableData.Comments.Count > 0)
            {
                var comments = new TreeNode("Comments:");
                foreach (var comment in _stringTableData.Comments)
                    comments.Items.Add(new TreeNode($"{comment:X8}"));

                treeNode.Add(comments);
            }

            return treeNode;
        }
    }
}
