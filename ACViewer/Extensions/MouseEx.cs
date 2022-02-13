using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace ACViewer.Extensions
{
    public class MouseEx
    {
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        public static Point SetCursor(Visual visual, int x, int y)
        {
            var p = visual.PointToScreen(new Point(x, y));
            SetCursorPos((int)p.X, (int)p.Y);
            return p;
        }
    }
}
