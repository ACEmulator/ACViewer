using System.Collections.Generic;

namespace ACViewer.Entity.AnimationHooks
{
    public class DiffuseHook : AnimationHook
    {
        public DiffuseHook(ACE.DatLoader.Entity.AnimationHook hook)
            : base(hook)
        {
        }

        public override List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_hook is ACE.DatLoader.Entity.AnimationHooks.DiffuseHook _diffuseHook)
            {
                treeNode.Add(new TreeNode($"Start: {_diffuseHook.Start}"));
                treeNode.Add(new TreeNode($"End: {_diffuseHook.End}"));
                treeNode.Add(new TreeNode($"Time: {_diffuseHook.Time}"));
            }
            treeNode.AddRange(base.BuildTree());

            return treeNode;
        }
    }
}
