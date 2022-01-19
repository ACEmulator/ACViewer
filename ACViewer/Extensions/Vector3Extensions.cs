using System;

using ACE.Server.Physics;

namespace ACViewer
{
    public static class Vector3Extensions
    {
        public static System.Numerics.Vector3 ToNumerics(this Microsoft.Xna.Framework.Vector3 v)
        {
            return new System.Numerics.Vector3(v.X, v.Y, v.Z);
        }

        public static Microsoft.Xna.Framework.Vector3 ToXna(this System.Numerics.Vector3 v)
        {
            return new Microsoft.Xna.Framework.Vector3(v.X, v.Y, v.Z);
        }

        public static bool IsZero(this System.Numerics.Vector3 v)
        {
            return v.X == 0 && v.Y == 0 && v.Z == 0;
        }

        public static bool IsZeroEpsilon(this System.Numerics.Vector3 v)
        {
            return Math.Abs(v.X) <= PhysicsGlobals.EPSILON && Math.Abs(v.Y) <= PhysicsGlobals.EPSILON && Math.Abs(v.Z) <= PhysicsGlobals.EPSILON;
        }

        public static bool NearZero(this System.Numerics.Vector3 v)
        {
            //return Math.Abs(v.X) <= PhysicsGlobals.EPSILON && Math.Abs(v.Y) <= PhysicsGlobals.EPSILON && Math.Abs(v.Z) <= PhysicsGlobals.EPSILON;
            return Math.Abs(v.X) <= 1.0f && Math.Abs(v.Y) <= 1.0f && Math.Abs(v.Z) <= 1.0f;
        }
    }
}
