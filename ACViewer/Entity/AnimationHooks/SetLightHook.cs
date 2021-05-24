using System.Collections.Generic;

namespace ACViewer.Entity.AnimationHooks
{
    public class SetLightHook : AnimationHook
    {
        public SetLightHook(ACE.DatLoader.Entity.AnimationHook hook)
            : base(hook)
        {
        }

        public override List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_hook is ACE.DatLoader.Entity.AnimationHooks.SetLightHook _setLightHook)
            {
                treeNode.Add(new TreeNode($"LightsOn: {_setLightHook.LightsOn}"));
            }
            treeNode.AddRange(base.BuildTree());

            return treeNode;
        }
    }
}
