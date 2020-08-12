using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class PortalPoly
    {
        public ACE.DatLoader.Entity.PortalPoly _portalPoly;

        public PortalPoly(ACE.DatLoader.Entity.PortalPoly portalPoly)
        {
            _portalPoly = portalPoly;
        }

        public override string ToString()
        {
            return $"PortalIdx: {_portalPoly.PortalIndex}, PolygonId: {_portalPoly.PolygonId}";
        }
    }
}
