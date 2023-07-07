using System;
using System.Numerics;

namespace ACViewer
{
    public static class QuaternionExtensions
    {
        public static Quaternion ToNumerics(this Microsoft.Xna.Framework.Quaternion q)
        {
            return new Quaternion(q.X, q.Y, q.Z, q.W);
        }

        public static Microsoft.Xna.Framework.Quaternion ToXna(this Quaternion q)
        {
            return new Microsoft.Xna.Framework.Quaternion(q.X, q.Y, q.Z, q.W);
        }

        public static Vector3 ToEulerAngles(this Quaternion q)
        {
            var angles = Vector3.Zero;

            // roll / x
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch / y
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
            {
                angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
            }
            else
            {
                angles.Y = (float)Math.Asin(sinp);
            }

            // yaw / z
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }
    }
}
