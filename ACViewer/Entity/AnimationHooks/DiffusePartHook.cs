using System.Collections.Generic;

namespace ACViewer.Entity.AnimationHooks
{
    public class DiffusePartHook : AnimationHook
    {
        public DiffusePartHook(ACE.DatLoader.Entity.AnimationHook hook)
            : base(hook)
        {
        }

        public override List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_hook is ACE.DatLoader.Entity.AnimationHooks.DiffusePartHook _diffusePartHook)
            {
                treeNode.Add(new TreeNode($"Part: {_diffusePartHook.Part}"));
                treeNode.Add(new TreeNode($"Start: {_diffusePartHook.Start}"));
                treeNode.Add(new TreeNode($"End: {_diffusePartHook.End}"));
                treeNode.Add(new TreeNode($"Time: {_diffusePartHook.Time}"));
            }
            treeNode.AddRange(base.BuildTree());

            return treeNode;
        }
    }
}
