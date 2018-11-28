﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACE.Entity.Enum;
using DatExplorer.Entity;

namespace DatExplorer.FileTypes
{
    public class Animation
    {
        public ACE.DatLoader.FileTypes.Animation _animation;

        public Animation(ACE.DatLoader.FileTypes.Animation animation)
        {
            _animation = animation;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_animation.Id:X8}");

            if (_animation.Flags.HasFlag(AnimationFlags.PosFrames))
            {
                var posFrames = new TreeNode("PosFrames");
                for (var i = 0; i < _animation.PosFrames.Count; i++)
                    posFrames.Items.Add(new TreeNode($"{i}: {new Frame(_animation.PosFrames[i])}"));

                treeView.Items.Add(posFrames);
            }

            var partFrames = new TreeNode("PartFrames");
            for (var i = 0; i < _animation.PartFrames.Count; i++)
            {
                var partFrame = new TreeNode($"{i}");
                partFrame.Items.AddRange(new AnimationFrame(_animation.PartFrames[i]).BuildTree());
                partFrames.Items.Add(partFrame);
            }
            treeView.Items.Add(partFrames);

            return treeView;
        }
    }
}
