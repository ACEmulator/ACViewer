using System.Collections.Generic;

namespace ACViewer.Entity.AnimationHooks
{
    public class TransparentPartHook : AnimationHook
    {
        public TransparentPartHook(ACE.DatLoader.Entity.AnimationHook hook)
            : base(hook)
        {
        }

        public override List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_hook is ACE.DatLoader.Entity.AnimationHooks.TransparentPartHook _transparentPartHook)
            {
                treeNode.Add(new TreeNode($"Part: {_transparentPartHook.Part}"));
                treeNode.Add(new TreeNode($"Start: {_transparentPartHook.Start}"));
                treeNode.Add(new TreeNode($"End: {_transparentPartHook.End}"));
                treeNode.Add(new TreeNode($"Time: {_transparentPartHook.Time}"));
            }
            treeNode.AddRange(base.BuildTree());

            return treeNode;
        }
    }
}
