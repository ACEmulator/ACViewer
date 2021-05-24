using System.Collections.Generic;

using ACViewer.Entity.AnimationHooks;

namespace ACViewer.Entity
{
    public class AnimationFrame
    {
        public ACE.DatLoader.Entity.AnimationFrame _animationFrame;

        public AnimationFrame(ACE.DatLoader.Entity.AnimationFrame animationFrame)
        {
            _animationFrame = animationFrame;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            var frames = new TreeNode("Frames:");
            foreach (var frame in _animationFrame.Frames)
                frames.Items.Add(new TreeNode(new Frame(frame).ToString()));

            treeNode.Add(frames);

            if (_animationFrame.Hooks.Count > 0)
            {
                if (_animationFrame.Hooks.Count == 1)
                {
                    var _hook = _animationFrame.Hooks[0];
                    
                    var hookNode = new TreeNode($"HookType: {_hook.HookType}");

                    var hook = AnimationHook.Create(_hook);
                    hookNode.Items.AddRange(hook.BuildTree());

                    treeNode.Add(hookNode);
                }
                else
                {
                    var hooks = new TreeNode("Hooks");

                    foreach (var _hook in _animationFrame.Hooks)
                    {
                        var hookNode = new TreeNode($"HookType: {_hook.HookType}");

                        var hook = AnimationHook.Create(_hook);
                        hookNode.Items.AddRange(hook.BuildTree());

                        hooks.Items.Add(hookNode);
                    }

                    treeNode.Add(hooks);
                }
            }
            return treeNode;
        }
    }
}
