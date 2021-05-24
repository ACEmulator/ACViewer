using System.Collections.Generic;

namespace ACViewer.Entity.AnimationHooks
{
    public class EtherealHook : AnimationHook
    {
        public EtherealHook(ACE.DatLoader.Entity.AnimationHook hook)
            : base(hook)
        {
        }

        public override List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_hook is ACE.DatLoader.Entity.AnimationHooks.EtherealHook _etherealHook)
            {
                treeNode.Add(new TreeNode($"Ethereal: {_etherealHook.Ethereal}"));
            }
            treeNode.AddRange(base.BuildTree());

            return treeNode;
        }
    }
}
