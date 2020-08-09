using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class DayGroup
    {
        public ACE.DatLoader.Entity.DayGroup _dayGroup;

        public DayGroup(ACE.DatLoader.Entity.DayGroup dayGroup)
        {
            _dayGroup = dayGroup;
        }

        public List<TreeNode> BuildTree()
        {
            var chanceOfOccur = new TreeNode($"ChanceOfOccur: {_dayGroup.ChanceOfOccur}");
            var weather = new TreeNode($"Weather: {_dayGroup.DayName}");

            var skyObjects = new TreeNode("SkyObjects:");
            for (var i = 0; i < _dayGroup.SkyObjects.Count; i++)
            {
                var skyObj = new TreeNode($"{i}");
                skyObj.Items.AddRange(new SkyObject(_dayGroup.SkyObjects[i]).BuildTree());
                skyObjects.Items.Add(skyObj);
            }

            var skyTimesOfDay = new TreeNode("SkyTimesOfDay:");
            for (var i = 0; i < _dayGroup.SkyTime.Count; i++)
            {
                var skyTimeOfDay = new TreeNode($"{i}");
                skyTimeOfDay.Items.AddRange(new SkyTimeOfDay(_dayGroup.SkyTime[i]).BuildTree());
                skyTimesOfDay.Items.Add(skyTimeOfDay);
            }
            return new List<TreeNode>() { chanceOfOccur, weather, skyObjects, skyTimesOfDay };
        }
    }
}
