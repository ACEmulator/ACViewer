using Microsoft.Xna.Framework;

namespace ACViewer.Model
{
    public class GfxObjInstance
    {
        public GfxObj GfxObj { get; set; }

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public GfxObjInstance(uint gfxObjID)
        {
            GfxObj = GfxObjCache.Get(gfxObjID);

            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
        }
    }
}
