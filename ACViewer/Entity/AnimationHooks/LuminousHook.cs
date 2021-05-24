using System.Collections.Generic;

namespace ACViewer.Entity.AnimationHooks
{
    public class LuminousHook : AnimationHook
    {
        public LuminousHook(ACE.DatLoader.Entity.AnimationHook hook)
            : base(hook)
        {
        }

        public override List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_hook is ACE.DatLoader.Entity.AnimationHooks.LuminousHook _luminousHook)
            {
                treeNode.Add(new TreeNode($"Start: {_luminousHook.Start}"));
                treeNode.Add(new TreeNode($"End: {_luminousHook.End}"));
                treeNode.Add(new TreeNode($"Time: {_luminousHook.Time}"));
            }
            treeNode.AddRange(base.BuildTree());

            return treeNode;
        }
    }
}
