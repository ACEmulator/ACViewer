using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class PhysicsScriptTableData
    {
        public ACE.DatLoader.Entity.PhysicsScriptTableData _data;

        public PhysicsScriptTableData(ACE.DatLoader.Entity.PhysicsScriptTableData data)
        {
            _data = data;
        }

        public List<TreeNode> BuildTree()
        {
            var scriptTable = new TreeNode("ScriptMods:");

            foreach (var scriptMod in _data.Scripts)
                scriptTable.Items.Add(new TreeNode($"{new ScriptMod(scriptMod)}", clickable: true));

            return scriptTable.Items;
        }
    }
}
