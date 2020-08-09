using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class GameTime
    {
        public ACE.DatLoader.Entity.GameTime _gameTime;

        public GameTime(ACE.DatLoader.Entity.GameTime gameTime)
        {
            _gameTime = gameTime;
        }

        public List<TreeNode> BuildTree()
        {
            var zeroTimeOfYear = new TreeNode($"ZeroTimeOfYear: {_gameTime.ZeroTimeOfYear}");
            var zeroYear = new TreeNode($"ZeroYear: {_gameTime.ZeroYear}");
            var dayLength = new TreeNode($"DayLength: {_gameTime.DayLength}");
            var daysPerYear = new TreeNode($"DaysPerYear: {_gameTime.DaysPerYear}");
            var yearSpec = new TreeNode($"YearSpec: {_gameTime.YearSpec}");

            var timesOfDay = new TreeNode($"TimesOfDay:");
            foreach (var timeOfDay in _gameTime.TimesOfDay)
            {
                var timeOfDayTree = new TimeOfDay(timeOfDay).BuildTree();
                var timeOfDayNode = new TreeNode(timeOfDayTree[2].Name.Replace("Name: ", ""));
                timeOfDayTree.RemoveAt(2);
                timeOfDayNode.Items.AddRange(timeOfDayTree);
                timesOfDay.Items.Add(timeOfDayNode);
            }

            var daysOfWeek = new TreeNode($"DaysOfWeek:");
            foreach (var dayOfWeek in _gameTime.DaysOfTheWeek)
                daysOfWeek.Items.Add(new TreeNode($"{dayOfWeek}"));

            var seasons = new TreeNode($"Seasons:");
            foreach (var season in _gameTime.Seasons)
            {
                var seasonTree = new Season(season).BuildTree();
                var seasonNode = new TreeNode(seasonTree[1].Name.Replace("Name: ", ""));
                seasonTree.RemoveAt(1);
                seasonNode.Items.AddRange(seasonTree);
                seasons.Items.Add(seasonNode);
            }

            return new List<TreeNode>() { zeroTimeOfYear, zeroYear, dayLength, daysPerYear, yearSpec, timesOfDay, daysOfWeek, seasons };
        }
    }
}
