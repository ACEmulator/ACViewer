using Microsoft.Xna.Framework;

namespace ACViewer.Model
{
    public class GfxObjInstance
    {
        public GfxObj GfxObj;

        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public GfxObjInstance(uint gfxObjID)
        {
            GfxObj = GfxObjCache.Get(gfxObjID);

            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
        }
    }
}
