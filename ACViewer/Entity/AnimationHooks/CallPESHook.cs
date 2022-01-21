using System.Collections.Generic;

namespace ACViewer.Entity.AnimationHooks
{
    public class CallPESHook : AnimationHook
    {
        public CallPESHook(ACE.DatLoader.Entity.AnimationHook hook)
            : base(hook)
        {
        }

        public override List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_hook is ACE.DatLoader.Entity.AnimationHooks.CallPESHook _callPESHook)
            {
                treeNode.Add(new TreeNode($"PES: {_callPESHook.PES:X8}", clickable: true));
                treeNode.Add(new TreeNode($"Pause: {_callPESHook.Pause}"));
            }
            treeNode.AddRange(base.BuildTree());

            return treeNode;
        }
    }
}
