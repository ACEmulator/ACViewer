using System.Collections.Generic;

namespace ACViewer.Entity.AnimationHooks
{
    public class SetOmegaHook : AnimationHook
    {
        public SetOmegaHook(ACE.DatLoader.Entity.AnimationHook hook)
            : base(hook)
        {
        }

        public override List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_hook is ACE.DatLoader.Entity.AnimationHooks.SetOmegaHook _setOmegaHook)
            {
                treeNode.Add(new TreeNode($"Axis: {_setOmegaHook.Axis}"));
            }
            treeNode.AddRange(base.BuildTree());

            return treeNode;
        }
    }
}
