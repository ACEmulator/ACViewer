using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                var hooks = new TreeNode("Hooks:");
                foreach (var hook in _animationFrame.Hooks)
                    hooks.Items.Add(new TreeNode(new AnimationHook(hook).ToString()));

                treeNode.Add(hooks);
            }
            return treeNode;
        }
    }
}
