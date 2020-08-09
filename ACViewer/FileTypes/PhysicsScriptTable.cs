using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACViewer.Entity;
using ACE.Entity.Enum;

namespace ACViewer.FileTypes
{
    public class PhysicsScriptTable
    {
        public ACE.DatLoader.FileTypes.PhysicsScriptTable _scriptTable;

        public PhysicsScriptTable(ACE.DatLoader.FileTypes.PhysicsScriptTable scriptTable)
        {
            _scriptTable = scriptTable;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_scriptTable.Id:X8}");

            foreach (var kvp in _scriptTable.ScriptTable)
            {
                var script = new TreeNode($"{(PlayScript)kvp.Key}");
                script.Items.AddRange(new PhysicsScriptTableData(kvp.Value).BuildTree());

                treeView.Items.Add(script);
            }
            return treeView;
        }
    }
}
