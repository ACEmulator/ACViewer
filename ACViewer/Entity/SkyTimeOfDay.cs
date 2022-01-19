using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class SkyTimeOfDay
    {
        public ACE.DatLoader.Entity.SkyTimeOfDay _skyTimeOfDay;

        public SkyTimeOfDay(ACE.DatLoader.Entity.SkyTimeOfDay skyTimeOfDay)
        {
            _skyTimeOfDay = skyTimeOfDay;
        }

        public List<TreeNode> BuildTree()
        {
            var begin = new TreeNode($"Begin: {_skyTimeOfDay.Begin}");
            var dirBright = new TreeNode($"DirBright: {_skyTimeOfDay.DirBright}");
            var dirHeading = new TreeNode($"DirHeading: {_skyTimeOfDay.DirHeading}");
            var dirPitch = new TreeNode($"DirPitch: {_skyTimeOfDay.DirPitch}");
            var dirColor = new TreeNode($"DirColor: {_skyTimeOfDay.DirColor:X8}");
            var ambientBright = new TreeNode($"AmbientBrightness: {_skyTimeOfDay.AmbBright}");
            var ambientColor = new TreeNode($"AmbientColor: {_skyTimeOfDay.AmbColor:X8}");
            var minFog = new TreeNode($"MinFog: {_skyTimeOfDay.MinWorldFog}");
            var maxFog = new TreeNode($"MaxFog: {_skyTimeOfDay.MaxWorldFog}");
            var fogColor = new TreeNode($"FogColor: {_skyTimeOfDay.WorldFogColor:X8}");
            var worldFog = new TreeNode($"Fog: {_skyTimeOfDay.WorldFog}");

            var skyObjReplace = new TreeNode($"SkyObjectReplace:");
            foreach (var obj in _skyTimeOfDay.SkyObjReplace)
            {
                var objTree = new SkyObjectReplace(obj).BuildTree();
                var objNode = new TreeNode(objTree[0].Name.Replace("ObjIdx: ", ""));
                objTree.RemoveAt(0);
                objNode.Items.AddRange(objTree);
                skyObjReplace.Items.Add(objNode);
            }

            return new List<TreeNode>() { begin, dirBright, dirHeading, dirPitch, dirColor, ambientBright, ambientColor, minFog, maxFog, fogColor, worldFog, skyObjReplace };
        }
    }
}
