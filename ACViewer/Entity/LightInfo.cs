using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class LightInfo
    {
        public ACE.DatLoader.Entity.LightInfo _lightInfo;

        public LightInfo(ACE.DatLoader.Entity.LightInfo lightInfo)
        {
            _lightInfo = lightInfo;
        }

        public override string ToString()
        {
            return $"Viewer space location: {_lightInfo.ViewerSpaceLocation}, Color: {_lightInfo.Color}, Intensity: {_lightInfo.Intensity}, Falloff: {_lightInfo.Falloff}, ConeAngle: {_lightInfo.ConeAngle}";
        }
    }
}
