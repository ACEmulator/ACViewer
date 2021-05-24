using System.Collections.Generic;

namespace ACViewer.Entity.AnimationHooks
{
    public class SoundTweakedHook : AnimationHook
    {
        public SoundTweakedHook(ACE.DatLoader.Entity.AnimationHook hook)
            : base(hook)
        {
        }

        public override List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_hook is ACE.DatLoader.Entity.AnimationHooks.SoundTweakedHook _soundTweakedHook)
            {
                treeNode.Add(new TreeNode($"SoundID: {_soundTweakedHook.SoundID:X8}"));
                treeNode.Add(new TreeNode($"Priority: {_soundTweakedHook.Priority}"));
                treeNode.Add(new TreeNode($"Probability: {_soundTweakedHook.Probability}"));
                treeNode.Add(new TreeNode($"Volume: {_soundTweakedHook.Volume}"));
            }
            treeNode.AddRange(base.BuildTree());

            return treeNode;
        }
    }
}
