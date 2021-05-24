using System.Collections.Generic;

using ACE.Entity.Enum;

namespace ACViewer.Entity.AnimationHooks
{
    public class SoundTableHook : AnimationHook
    {
        public SoundTableHook(ACE.DatLoader.Entity.AnimationHook hook)
            : base(hook)
        {
        }

        public override List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_hook is ACE.DatLoader.Entity.AnimationHooks.SoundTableHook _soundTableHook)
            {
                treeNode.Add(new TreeNode($"SoundType: {(Sound)_soundTableHook.SoundType}"));
            }
            treeNode.AddRange(base.BuildTree());

            return treeNode;
        }
    }
}
