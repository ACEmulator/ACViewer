using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class SoundTableData
    {
        public ACE.DatLoader.Entity.SoundTableData _data;

        public SoundTableData(ACE.DatLoader.Entity.SoundTableData data)
        {
            _data = data;
        }

        public List<TreeNode> BuildTree()
        {
            var soundID = new TreeNode($"Sound ID: {_data.SoundId:X8}", clickable: true);
            var priority = new TreeNode($"Priority: {_data.Priority}");
            var probability = new TreeNode($"Probability: {_data.Probability}");
            var volume = new TreeNode($"Volume: {_data.Volume}");

            return new List<TreeNode>() { soundID, priority, probability, volume };
        }
    }
}
