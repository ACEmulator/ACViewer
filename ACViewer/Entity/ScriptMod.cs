using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class ScriptMod
    {
        public ACE.DatLoader.Entity.ScriptAndModData _scriptMod;

        public ScriptMod(ACE.DatLoader.Entity.ScriptAndModData scriptMod)
        {
            _scriptMod = scriptMod;
        }

        public List<TreeNode> BuildTree()
        {
            var mod = new TreeNode($"{_scriptMod.Mod}");
            var script = new TreeNode($"{_scriptMod.ScriptId:X8}", clickable: true);

            return new List<TreeNode>() { mod, script };
        }

        public override string ToString()
        {
            return $"Mod: {_scriptMod.Mod}, Script: {_scriptMod.ScriptId:X8}";
        }
    }
}
