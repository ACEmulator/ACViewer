using System.Collections.Generic;

namespace ACViewer.Entity.AnimationHooks
{
    public class TextureVelocityHook : AnimationHook
    {
        public TextureVelocityHook(ACE.DatLoader.Entity.AnimationHook hook)
            : base(hook)
        {
        }

        public override List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_hook is ACE.DatLoader.Entity.AnimationHooks.TextureVelocityHook _textureVelocityHook)
            {
                treeNode.Add(new TreeNode($"USpeed: {_textureVelocityHook.USpeed}"));
                treeNode.Add(new TreeNode($"VSpeed: {_textureVelocityHook.VSpeed}"));
            }
            treeNode.AddRange(base.BuildTree());

            return treeNode;
        }
    }
}
