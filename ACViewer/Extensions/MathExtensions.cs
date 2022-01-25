namespace ACViewer.Extensions
{
    public static class MathExtensions
    {
        public static float Clamp(float val, float min, float max)
        {
            if (val < min)
                val = min;
            else if (val > max)
                val = max;

            return val;
        }
    }
}
