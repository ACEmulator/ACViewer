using System;
using System.IO;

using Newtonsoft.Json;

namespace ACViewer.Config
{
    public static class ConfigManager
    {
        private static readonly string Filename = "ACViewer.json";
        
        private static Config config { get; set; }

        public static Config Config
        {
            get
            {
                if (config == null)
                    config = new Config();

                return config;
            }
        }

        public static Config Snapshot { get; set; }

        public static void SaveConfig()
        {
            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            
            var json = JsonConvert.SerializeObject(config, settings);

            File.WriteAllText(Filename, json);
        }

        public static void LoadConfig()
        {
            config = ReadConfig();
        }

        public static Config ReadConfig()
        {
            if (!File.Exists(Filename)) return null;

            var json = File.ReadAllText(Filename);

            var _config = JsonConvert.DeserializeObject<Config>(json);

            if (_config == null)
            {
                Console.WriteLine($"ConfigManager.LoadConfig() - failed to parse {Filename}");
                return null;
            }
            return _config;
        }

        public static void TakeSnapshot()
        {
            Snapshot = ReadConfig();
        }

        public static void RestoreSnapshot()
        {
            config = Snapshot;
        }

        public static bool HasDBInfo
        {
            get
            {
                return config != null && config.Database != null && !string.IsNullOrWhiteSpace(config.Database.Host) &&
                    config.Database.Port > 0 &&
                    !string.IsNullOrWhiteSpace(config.Database.DatabaseName) &&
                    !string.IsNullOrWhiteSpace(config.Database.Username) &&
                    !string.IsNullOrWhiteSpace(config.Database.Password);
            }
        }
    }
}
