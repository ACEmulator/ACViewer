using ACE.Entity.Enum;

using ACViewer.Entity;

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
