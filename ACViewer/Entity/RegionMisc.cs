using System.Collections.Generic;

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
            var gameMapID = new TreeNode($"GameMapID: {_regionMisc.GameMapID:X8}", clickable: true);
            var autoTestMapID = new TreeNode($"AutoTest MapID: {_regionMisc.AutotestMapId:X8}", clickable: true);
            var autoTestMapSize = new TreeNode($"AutoTest MapSize: {_regionMisc.AutotestMapSize}");
            var clearCellID = new TreeNode($"ClearCellID: {_regionMisc.ClearCellId:X8}", clickable: true);
            var clearMonsterID = new TreeNode($"ClearMonsterID: {_regionMisc.ClearMonsterId:X8}", clickable: true);

            return new List<TreeNode>() { version, gameMapID, autoTestMapID, autoTestMapSize, clearCellID, clearMonsterID };
        }
    }
}
