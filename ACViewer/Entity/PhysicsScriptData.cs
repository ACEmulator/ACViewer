using System.Collections.Generic;

using ACViewer.Entity.AnimationHooks;

namespace ACViewer.Entity
{
    public class PhysicsScriptData
    {
        public ACE.DatLoader.Entity.PhysicsScriptData _scriptData;

        public PhysicsScriptData(ACE.DatLoader.Entity.PhysicsScriptData scriptData)
        {
            _scriptData = scriptData;
        }

        public List<TreeNode> BuildTree()
        {
            var startTime = new TreeNode($"StartTime: {_scriptData.StartTime}");

            var hook = AnimationHook.Create(_scriptData.Hook);

            var hookNode = new TreeNode($"Hook:", hook.BuildTree());

            return new List<TreeNode>() { startTime, hookNode };
        }
    }
}
