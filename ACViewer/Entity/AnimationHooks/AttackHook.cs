using System.Collections.Generic;

namespace ACViewer.Entity.AnimationHooks
{
    public class AttackHook : AnimationHook
    {
        public AttackHook(ACE.DatLoader.Entity.AnimationHook hook)
            : base(hook)
        {
        }

        public override List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_hook is ACE.DatLoader.Entity.AnimationHooks.AttackHook _attackHook)
            {
                var attackCone = new AttackCone(_attackHook.AttackCone);
                treeNode.AddRange(attackCone.BuildTree());
            }
            treeNode.AddRange(base.BuildTree());

            return treeNode;
        }
    }
}
