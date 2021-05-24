using System.Collections.Generic;

namespace ACViewer.Entity.AnimationHooks
{
    public class TextureVelocityPartHook : AnimationHook
    {
        public TextureVelocityPartHook(ACE.DatLoader.Entity.AnimationHook hook)
            : base(hook)
        {
        }

        public override List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_hook is ACE.DatLoader.Entity.AnimationHooks.TextureVelocityPartHook _textureVelocityPartHook)
            {
                treeNode.Add(new TreeNode($"PartIndex: {_textureVelocityPartHook.PartIndex}"));
                treeNode.Add(new TreeNode($"USpeed: {_textureVelocityPartHook.USpeed}"));
                treeNode.Add(new TreeNode($"VSpeed: {_textureVelocityPartHook.VSpeed}"));
            }
            treeNode.AddRange(base.BuildTree());

            return treeNode;
        }
    }
}
