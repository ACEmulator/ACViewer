using Microsoft.Xna.Framework;
using ACE.Server.Physics.Common;

namespace ACViewer
{
    public static class PositionExtensions
    {
        public static Matrix ToXna(this Position pos)
        {
            var frameTransform = pos.Frame.ToXna();
            
            // translate to landblock
            var lbx = pos.ObjCellID >> 24;
            var lby = pos.ObjCellID >> 16 & 0xFF;

            var worldTranslate = Matrix.CreateTranslation(new Vector3(lbx * LandDefs.BlockLength, lby * LandDefs.BlockLength, 0));

            return frameTransform * worldTranslate;
        }

        public static Vector3 GetWorldPos(this Position pos)
        {
            // translate to landblock
            var lbx = pos.ObjCellID >> 24;
            var lby = pos.ObjCellID >> 16 & 0xFF;

            return new Vector3(lbx * LandDefs.BlockLength + pos.Frame.Origin.X, lby * LandDefs.BlockLength + pos.Frame.Origin.Y, pos.Frame.Origin.Z);
        }
    }
}
