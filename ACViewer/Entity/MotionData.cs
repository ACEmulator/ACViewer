using System.Collections.Generic;

using ACE.Entity.Enum;

namespace ACViewer.Entity
{
    public class MotionData
    {
        public ACE.DatLoader.Entity.MotionData _motionData;

        public MotionData(ACE.DatLoader.Entity.MotionData motionData)
        {
            _motionData = motionData;
        }

        public List<TreeNode> BuildTree()
        {
            var treeNode = new List<TreeNode>();

            if (_motionData.Bitfield != 0)
            {
                var bitfield = new TreeNode($"Bitfield: {_motionData.Bitfield:X8}");
                treeNode.Add(bitfield);
            }

            if (_motionData.Anims.Count > 0)
            {
                if (_motionData.Anims.Count == 1)
                {
                    var anim = new TreeNode("Animation:");
                    anim.Items.AddRange(new AnimData(_motionData.Anims[0]).BuildTree());
                    treeNode.Add(anim);
                }
                else
                {
                    var anims = new TreeNode("Animations:");
                    for (var i = 0; i < _motionData.Anims.Count; i++)
                    {
                        var anim = new TreeNode($"{i}");
                        anim.Items.AddRange(new AnimData(_motionData.Anims[i]).BuildTree());
                        anims.Items.Add(anim);
                    }
                    treeNode.Add(anims);
                }
            }

            if (_motionData.Flags.HasFlag(MotionDataFlags.HasVelocity))
            {
                var velocity = new TreeNode($"Velocity: {_motionData.Velocity}");
                treeNode.Add(velocity);
            }

            if (_motionData.Flags.HasFlag(MotionDataFlags.HasOmega))
            {
                var omega = new TreeNode($"Omega: {_motionData.Omega}");
                treeNode.Add(omega);
            }
            return treeNode;
        }
    }
}
