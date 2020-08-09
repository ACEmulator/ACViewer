namespace ACViewer
{
    public static class QuaternionExtensions
    {
        public static System.Numerics.Quaternion ToNumerics(this Microsoft.Xna.Framework.Quaternion q)
        {
            return new System.Numerics.Quaternion(q.X, q.Y, q.Z, q.W);
        }

        public static Microsoft.Xna.Framework.Quaternion ToXna(this System.Numerics.Quaternion q)
        {
            return new Microsoft.Xna.Framework.Quaternion(q.X, q.Y, q.Z, q.W);
        }
    }
}
