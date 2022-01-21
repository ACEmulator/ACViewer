using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class AmbientSoundTableDesc
    {
        public ACE.DatLoader.Entity.AmbientSTBDesc _stbDesc;

        public AmbientSoundTableDesc(ACE.DatLoader.Entity.AmbientSTBDesc stbDesc)
        {
            _stbDesc = stbDesc;
        }

        public List<TreeNode> BuildTree()
        {
            var ambientSoundTableID = new TreeNode($"Ambient Sound Table ID: {_stbDesc.STBId:X8}", clickable: true);

            var ambientSounds = new TreeNode("Ambient Sounds:");
            for (var i = 0; i < _stbDesc.AmbientSounds.Count; i++)
            {
                var sound = new TreeNode($"{i}");
                sound.Items.AddRange(new AmbientSoundDesc(_stbDesc.AmbientSounds[i]).BuildTree());

                ambientSounds.Items.Add(sound);
            }
            return new List<TreeNode>() { ambientSoundTableID, ambientSounds };
        }
    }
}
