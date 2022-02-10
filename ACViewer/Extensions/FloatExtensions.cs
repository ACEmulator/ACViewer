using System;

namespace ACViewer.Extensions
{
    public static class FloatExtensions
    {
        /// <summary>
        /// Converts radians to degrees
        /// </summary>
        public static float ToDegs(this float rads)
        {
            return (float)(180.0f / Math.PI * rads);
        }

        /// <summary>
        /// Converts degrees to radians
        /// </summary>
        public static float ToRads(this float degrees)
        {
            return (float)(Math.PI / 180.0f * degrees);
        }
    }
}
