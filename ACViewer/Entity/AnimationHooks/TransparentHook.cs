using System.Collections.Generic;

namespace ACViewer.Entity.AnimationHooks
{
    public class TransparentHook : AnimationHook
    {
        public TransparentHook(ACE.DatLoader.Entity.AnimationHook hook)
            : base(hook)
        {
        }

        public override List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_hook is ACE.DatLoader.Entity.AnimationHooks.TransparentHook _transparentHook)
            {
                treeNode.Add(new TreeNode($"Start: {_transparentHook.Start}"));
                treeNode.Add(new TreeNode($"End: {_transparentHook.End}"));
                treeNode.Add(new TreeNode($"Time: {_transparentHook.Time}"));
            }
            treeNode.AddRange(base.BuildTree());

            return treeNode;
        }
    }
}
