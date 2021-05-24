using System.Collections.Generic;

namespace ACViewer.Entity.AnimationHooks
{
    public class ReplaceObjectHook : AnimationHook
    {
        public ReplaceObjectHook(ACE.DatLoader.Entity.AnimationHook hook)
            : base(hook)
        {
        }

        public override List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_hook is ACE.DatLoader.Entity.AnimationHooks.ReplaceObjectHook _replaceObjectHook)
            {
                var animPartChange = new AnimPartChange(_replaceObjectHook.APChange);
                treeNode.AddRange(animPartChange.BuildTree());
            }
            treeNode.AddRange(base.BuildTree());

            return treeNode;
        }
    }
}
