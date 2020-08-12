using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class TimeOfDay
    {
        public ACE.DatLoader.Entity.TimeOfDay _timeOfDay;

        public TimeOfDay(ACE.DatLoader.Entity.TimeOfDay timeOfDay)
        {
            _timeOfDay = timeOfDay;
        }

        public List<TreeNode> BuildTree()
        {
            var start = new TreeNode($"Start: {_timeOfDay.Start}");
            var isNight = new TreeNode($"IsNight: {_timeOfDay.IsNight}");
            var name = new TreeNode($"Name: {_timeOfDay.Name}");

            return new List<TreeNode>() { start, isNight, name };
        }
    }
}
