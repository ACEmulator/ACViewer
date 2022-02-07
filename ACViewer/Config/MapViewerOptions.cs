using ACViewer.Enum;

namespace ACViewer.Config
{
    public class MapViewerOptions
    {
        public MapViewerMode Mode { get; set; }

        public MapViewerOptions()
        {
            // defaults
            Mode = MapViewerMode.PreGenerated;
        }
    }
}
