using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACE.Entity.Enum;

namespace ACViewer.Entity
{
    public class AmbientSoundDesc
    {
        public ACE.DatLoader.Entity.AmbientSoundDesc _soundDesc;

        public AmbientSoundDesc(ACE.DatLoader.Entity.AmbientSoundDesc soundDesc)
        {
            _soundDesc = soundDesc;
        }

        public List<TreeNode> BuildTree()
        {
            var soundType = new TreeNode($"SoundType: {(Sound)_soundDesc.SType}");
            var volume = new TreeNode($"Volume: {_soundDesc.Volume}");
            var baseChance = new TreeNode($"BaseChance: {_soundDesc.BaseChance}");
            var minRate = new TreeNode($"MinRate: {_soundDesc.MinRate}");
            var maxRate = new TreeNode($"MaxRate: {_soundDesc.MaxRate}");

            return new List<TreeNode>() { soundType, volume, baseChance, minRate, maxRate };
        }
    }
}
