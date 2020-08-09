using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class CylSphere
    {
        public ACE.DatLoader.Entity.CylSphere _cylSphere;

        public CylSphere(ACE.DatLoader.Entity.CylSphere cylSphere)
        {
            _cylSphere = cylSphere;
        }

        public override string ToString()
        {
            return $"Origin: {_cylSphere.Origin}, Radius: {_cylSphere.Radius}, Height: {_cylSphere.Height}";
        }
    }
}
