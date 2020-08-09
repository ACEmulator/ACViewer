using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACE.Server.Physics;

namespace ACViewer.Render
{
    public class R_PhysicsPart
    {
        // a physics part has 1 gfxobj
        // a gfxobj can have multiple polygons
        public PhysicsPart PhysicsPart;
        public R_GfxObj R_GfxObj;

        public R_PhysicsPart(PhysicsPart physicsPart)
        {
            PhysicsPart = physicsPart;
            R_GfxObj = new R_GfxObj(physicsPart.GfxObj, PhysicsPart.GfxObjScale.ToXna());
        }
    }
}
