using System;

using ACE.Server.Physics;

namespace ACViewer
{
    public static class Vector2Extensions
    {
        public static System.Numerics.Vector2 ToNumerics(this Microsoft.Xna.Framework.Vector2 v)
        {
            return new System.Numerics.Vector2(v.X, v.Y);
        }

        /*public static Microsoft.Xna.Framework.Vector2 ToXna(this System.Numerics.Vector2 v)
        {
            return new Microsoft.Xna.Framework.Vector2(v.X, v.Y);
        }*/

        public static Microsoft.Xna.Framework.Vector3 ToXna(this System.Numerics.Vector2 v, int offset = 0)
        {
            return new Microsoft.Xna.Framework.Vector3(v.X, v.Y, offset);
        }

        public static bool IsZero(this System.Numerics.Vector2 v)
        {
            return v.X == 0 && v.Y == 0;
        }

        public static bool IsZeroEpsilon(this System.Numerics.Vector2 v)
        {
            return Math.Abs(v.X) <= PhysicsGlobals.EPSILON && Math.Abs(v.Y) <= PhysicsGlobals.EPSILON;
        }

        public static bool NearZero(this System.Numerics.Vector2 v)
        {
            return Math.Abs(v.X) <= 1.0f && Math.Abs(v.Y) <= 1.0f;
        }
    }
}
