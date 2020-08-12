using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class AnimData
    {
        public ACE.DatLoader.Entity.AnimData _animData;

        public AnimData(ACE.DatLoader.Entity.AnimData animData)
        {
            _animData = animData;
        }

        public List<TreeNode> BuildTree()
        {
            var animID = new TreeNode($"AnimID: {_animData.AnimId:X8}");
            var lowFrame = new TreeNode($"Low frame: {_animData.LowFrame}");
            var highFrame = new TreeNode($"High frame: {_animData.HighFrame}");
            var framerate = new TreeNode($"Framerate: {_animData.Framerate}");

            return new List<TreeNode>() { animID, lowFrame, highFrame, framerate };
        }
    }
}
