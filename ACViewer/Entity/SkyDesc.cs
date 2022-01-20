using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class SkyDesc
    {
        public ACE.DatLoader.Entity.SkyDesc _skyDesc;

        public SkyDesc(ACE.DatLoader.Entity.SkyDesc skyDesc)
        {
            _skyDesc = skyDesc;
        }

        public List<TreeNode> BuildTree()
        {
            var tickSize = new TreeNode($"TickSize: {_skyDesc.TickSize}");
            var lightTickSize = new TreeNode($"LightTickSize: {_skyDesc.LightTickSize}");

            var dayGroups = new TreeNode("DayGroups:");
            for (var i = 0; i < _skyDesc.DayGroups.Count; i++)
            {
                var dayGroup = new TreeNode($"{i:D2}");
                dayGroup.Items.AddRange(new DayGroup(_skyDesc.DayGroups[i]).BuildTree());
                dayGroups.Items.Add(dayGroup);
            }
            return new List<TreeNode>() { tickSize, lightTickSize, dayGroups };
        }
    }
}
