using ACViewer.Entity;
using ACViewer.Entity.AnimationHooks;

namespace ACViewer.FileTypes
{
    public class PhysicsScript
    {
        public ACE.DatLoader.FileTypes.PhysicsScript _playScript;

        public PhysicsScript(ACE.DatLoader.FileTypes.PhysicsScript playScript)
        {
            _playScript = playScript;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_playScript.Id:X8}");

            var scripts = new TreeNode("Scripts:");

            for (var i = 0; i < _playScript.ScriptData.Count; i++)
            {
                var scriptData = new PhysicsScriptData(_playScript.ScriptData[i]);

                var scriptNode = new TreeNode($"HookType: {scriptData._scriptData.Hook.HookType}, StartTime: {scriptData._scriptData.StartTime}");

                var animationHook = AnimationHook.Create(scriptData._scriptData.Hook);

                scriptNode.Items.AddRange(animationHook.BuildTree());

                scripts.Items.Add(scriptNode);
            }
            treeView.Items.AddRange(scripts.Items);
            return treeView;
        }
    }
}
