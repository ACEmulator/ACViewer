namespace ACViewer.Config
{
    public class Database
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string DatabaseName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public Database()
        {
            // defaults
            Host = "127.0.0.1";
            Port = 3306;
            DatabaseName = "ace_world";
        }
    }
}
