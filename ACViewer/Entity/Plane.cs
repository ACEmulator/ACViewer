using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class Plane
    {
        public ACE.DatLoader.Entity.Plane _plane;

        public Plane(ACE.DatLoader.Entity.Plane plane)
        {
            _plane = plane;
        }

        public override string ToString()
        {
            return $"Normal: {_plane.N} - Distance: {_plane.D}";
        }
    }
}
