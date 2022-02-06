using System.Windows.Media;

namespace ACViewer.Extensions
{
    public static class ColorExtensions
    {
        public static SolidColorBrush ToSolidColorBrush(this System.Drawing.Color c)
        {
            return new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
        }

        public static SolidColorBrush ToSolidColorBrush(this Microsoft.Xna.Framework.Color c)
        {
            return new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
        }

        public static System.Drawing.Color ToColor(this SolidColorBrush brush)
        {
            return System.Drawing.Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
        }

        public static Microsoft.Xna.Framework.Color ToXNAColor(this SolidColorBrush brush)
        {
            return new Microsoft.Xna.Framework.Color(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
        }
    }
}
