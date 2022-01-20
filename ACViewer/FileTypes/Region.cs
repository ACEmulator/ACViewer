using System.Collections.Generic;

using ACViewer.Entity;

namespace ACViewer.FileTypes
{
    public class Region
    {
        public ACE.DatLoader.FileTypes.RegionDesc _region;

        public Region(ACE.DatLoader.FileTypes.RegionDesc region)
        {
            _region = region;
        }

        public TreeNode BuildTree()
        {
            var treeView = new TreeNode($"{_region.Id:X8}");

            var regionNum = new TreeNode($"RegionNum: {_region.RegionNumber}");
            var version = new TreeNode($"Version: {_region.Version}");
            var name = new TreeNode($"Name: {_region.RegionName}");

            var landDefs = new TreeNode($"LandDefs:");
            landDefs.Items.AddRange(new LandDefs(_region.LandDefs).BuildTree());

            var gameTime = new TreeNode($"GameTime:");
            gameTime.Items.AddRange(new GameTime(_region.GameTime).BuildTree());

            var partsMask = new TreeNode($"PartsMask: {_region.PartsMask:X8}");

            treeView.Items.AddRange(new List<TreeNode>() { regionNum, version, name, landDefs, gameTime });

            if ((_region.PartsMask & 0x10) != 0)
            {
                var skyInfo = new TreeNode("SkyInfo:");
                skyInfo.Items.AddRange(new SkyDesc(_region.SkyInfo).BuildTree());

                treeView.Items.Add(skyInfo);
            }

            if ((_region.PartsMask & 0x01) != 0)
            {
                var soundInfo = new TreeNode("SoundInfo:");
                soundInfo.Items.AddRange(new SoundDesc(_region.SoundInfo).BuildTree());

                treeView.Items.Add(soundInfo);
            }

            if ((_region.PartsMask & 0x02) != 0)
            {
                var sceneInfo = new TreeNode("SceneInfo:");
                sceneInfo.Items.AddRange(new SceneDesc(_region.SceneInfo).BuildTree());

                treeView.Items.Add(sceneInfo);
            }

            var terrainInfo = new TreeNode("TerrainInfo:");
            terrainInfo.Items.AddRange(new TerrainDesc(_region.TerrainInfo).BuildTree());
            treeView.Items.Add(terrainInfo);

            if ((_region.PartsMask & 0x200) != 0)
            {
                var regionMisc = new TreeNode("RegionMisc:");
                regionMisc.Items.AddRange(new RegionMisc(_region.RegionMisc).BuildTree());

                treeView.Items.Add(regionMisc);
            }

            return treeView;
        }
    }
}
