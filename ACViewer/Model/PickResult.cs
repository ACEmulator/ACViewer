using System;

using ACE.Server.Physics;
using ACE.Server.Physics.Common;

using ACViewer.Enum;

namespace ACViewer.Model
{
    public class PickResult
    {
        public PickType Type { get; set; }
        public ObjCell ObjCell { get; set; }
        public PhysicsObj PhysicsObj { get; set; }
        public int PartIdx { get; set; }
        public ushort PolyIdx { get; set; }

        public ACE.Server.Physics.Polygon HitPoly { get; set; }    // only needed for ObjCell single poly mode

        public DateTime ClickTime { get; set; }

        public static readonly TimeSpan DoubleClickTime = TimeSpan.FromSeconds(0.5f);

        public PickResult()
        {
            ClickTime = DateTime.Now;
        }
    }
}
