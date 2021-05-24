using System.Collections.Generic;

namespace ACViewer.Entity.AnimationHooks
{
    public class DestroyParticleHook : AnimationHook
    {
        public DestroyParticleHook(ACE.DatLoader.Entity.AnimationHook hook)
            : base(hook)
        {
        }

        public override List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_hook is ACE.DatLoader.Entity.AnimationHooks.DestroyParticleHook _destroyParticleHook)
            {
                treeNode.Add(new TreeNode($"EmitterId: {_destroyParticleHook.EmitterId}"));
            }
            treeNode.AddRange(base.BuildTree());

            return treeNode;
        }
    }
}
