using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class RegionMisc
    {
        public ACE.DatLoader.Entity.RegionMisc _regionMisc;

        public RegionMisc(ACE.DatLoader.Entity.RegionMisc regionMisc)
        {
            _regionMisc = regionMisc;
        }

        public List<TreeNode> BuildTree()
        {
            var version = new TreeNode($"Version: {_regionMisc.Version}");
            var gameMapID = new TreeNode($"GameMapID: {_regionMisc.GameMapID:X8}");
            var autoTestMapID = new TreeNode($"AutoTest MapID: {_regionMisc.AutotestMapId:X8}");
            var autoTestMapSize = new TreeNode($"AutoTest MapSize: {_regionMisc.AutotestMapSize}");
            var clearCellID = new TreeNode($"ClearCellID: {_regionMisc.ClearCellId:X8}");
            var clearMonsterID = new TreeNode($"ClearMonsterID: {_regionMisc.ClearMonsterId:X8}");

            return new List<TreeNode>() { version, gameMapID, autoTestMapID, autoTestMapSize, clearCellID, clearMonsterID };
        }
    }
}
