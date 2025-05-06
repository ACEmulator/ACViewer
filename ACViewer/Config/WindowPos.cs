namespace ACViewer.Config
{
    public class WindowPos
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int VSplit { get; set; }
        public int HSplit { get; set; }
        public bool IsMaximized { get; set; }

        public WindowPos()
        {
            // defaults
            X = int.MinValue;
            Y = int.MinValue;
            Width = 1342;
            Height = 798;
            VSplit = 360;
            HSplit = 67;
            IsMaximized = false;
        }
    }
}
