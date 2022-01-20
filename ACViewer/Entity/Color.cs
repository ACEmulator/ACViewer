using System.Text;

namespace ACViewer.Entity
{
    public class Color
    {
        public static string ToRGBA(uint color)
        {
            // palette colors are natively stored in ARGB format
            var a = color >> 24;
            var r = (color >> 16) & 0xFF;
            var g = (color >> 8) & 0xFF;
            var b = color & 0xFF;

            var sb = new StringBuilder();
            sb.Append($"R: {r} G: {g} B: {b}");

            if (a < 255)
                sb.Append($" A: {a} ");

            return sb.ToString();
        }
    }
}
