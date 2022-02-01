namespace ACViewer.Config
{
    public class Config
    {
        public string ACFolder { get; set; }
        public bool AutomaticallyLoadDATsOnStartup { get; set; }
        public Database Database { get; set; } = new Database();
    }
}
