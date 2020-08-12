using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class Sphere
    {
        public ACE.DatLoader.Entity.Sphere _sphere;

        public Sphere(ACE.DatLoader.Entity.Sphere sphere)
        {
            _sphere = sphere;
        }

        public override string ToString()
        {
            return $"Origin: {_sphere.Origin}, Radius: {_sphere.Radius}";
        }
    }
}
