using System.Collections.Generic;

namespace ACViewer.Entity
{
    public class LightInfo
    {
        public ACE.DatLoader.Entity.LightInfo _lightInfo;

        public LightInfo(ACE.DatLoader.Entity.LightInfo lightInfo)
        {
            _lightInfo = lightInfo;
        }

        public List<TreeNode> BuildTree()
        {
            var viewerSpaceLocation = new TreeNode($"Viewer space location: {_lightInfo.ViewerSpaceLocation}");

            var color = new TreeNode($"Color: {Color.ToRGBA(_lightInfo.Color)}");
            
            var intensity = new TreeNode($"Intensity: {_lightInfo.Intensity}");

            var falloff = new TreeNode($"Falloff: {_lightInfo.Falloff}");

            var coneAngle = new TreeNode($"ConeAngle: {_lightInfo.ConeAngle}");

            return new List<TreeNode>() { viewerSpaceLocation, color, intensity, falloff, coneAngle };
        }

        public override string ToString()
        {
            return $"Viewer space location: {_lightInfo.ViewerSpaceLocation}, Color: {Color.ToRGBA(_lightInfo.Color)}, Intensity: {_lightInfo.Intensity}, Falloff: {_lightInfo.Falloff}, ConeAngle: {_lightInfo.ConeAngle}";
        }
    }
}
