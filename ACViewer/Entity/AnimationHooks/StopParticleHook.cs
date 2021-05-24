using System.Collections.Generic;

namespace ACViewer.Entity.AnimationHooks
{
    public class StopParticleHook : AnimationHook
    {
        public StopParticleHook(ACE.DatLoader.Entity.AnimationHook hook)
            : base(hook)
        {
        }

        public override List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_hook is ACE.DatLoader.Entity.AnimationHooks.StopParticleHook _stopParticleHook)
            {
                treeNode.Add(new TreeNode($"EmitterId: {_stopParticleHook.EmitterId}"));
            }
            treeNode.AddRange(base.BuildTree());

            return treeNode;
        }
    }
}
