using Microsoft.Xna.Framework;
using ACE.Server.Physics.Common;

namespace DatExplorer
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
    }
}
