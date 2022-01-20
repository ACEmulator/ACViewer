using ACE.Server.Physics;

namespace ACViewer.Render
{
    public class R_PhysicsPart
    {
        // a physics part has 1 gfxobj
        // a gfxobj can have multiple polygons
        public PhysicsPart PhysicsPart { get; set; }
        public R_GfxObj R_GfxObj { get; set; }

        public R_PhysicsPart(PhysicsPart physicsPart)
        {
            PhysicsPart = physicsPart;
            R_GfxObj = new R_GfxObj(physicsPart.GfxObj, PhysicsPart.GfxObjScale.ToXna());
        }
    }
}
