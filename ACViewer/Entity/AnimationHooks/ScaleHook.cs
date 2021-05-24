using System.Collections.Generic;

namespace ACViewer.Entity.AnimationHooks
{
    public class ScaleHook : AnimationHook
    {
        public ScaleHook(ACE.DatLoader.Entity.AnimationHook hook)
            : base(hook)
        {
        }

        public override List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_hook is ACE.DatLoader.Entity.AnimationHooks.ScaleHook _scaleHook)
            {
                treeNode.Add(new TreeNode($"End: {_scaleHook.End}"));
                treeNode.Add(new TreeNode($"Time: {_scaleHook.Time}"));
            }
            treeNode.AddRange(base.BuildTree());

            return treeNode;
        }
    }
}
