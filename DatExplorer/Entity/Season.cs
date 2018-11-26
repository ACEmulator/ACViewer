﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatExplorer.Entity
{
    public class Season
    {
        public ACE.DatLoader.Entity.Season _season;

        public Season(ACE.DatLoader.Entity.Season season)
        {
            _season = season;
        }

        public List<TreeNode> BuildTree()
        {
            var startDate = new TreeNode($"StartDate: {_season.StartDate}");
            var name = new TreeNode($"Name: {_season.Name}");

            return new List<TreeNode>() { startDate, name };
        }
    }
}
