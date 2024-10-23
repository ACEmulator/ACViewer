using ACViewer.Enum;

namespace ACViewer.Config
{
    public class MapViewerOptions
    {
        public MapViewerMode Mode { get; set; }
        public bool EnableZSlicing { get; set; } = false;
        public int CurrentZLevel { get; set; } = 1;
        public float LevelHeight { get; set; } = 10.0f;

        public MapViewerOptions()
        {
            // defaults
            Mode = MapViewerMode.PreGenerated;
        }
    }
}
