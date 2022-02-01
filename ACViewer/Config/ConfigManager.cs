using System;
using System.IO;

using Newtonsoft.Json;

namespace ACViewer.Config
{
    public static class ConfigManager
    {
        private static string Filename = "ACViewer.json";
        
        private static Config config;

        public static Config Config
        {
            get
            {
                if (config == null)
                    config = new Config();

                return config;
            }
        }

        public static void SaveConfig()
        {
            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            
            var json = JsonConvert.SerializeObject(config, settings);

            File.WriteAllText(Filename, json);
        }

        public static void LoadConfig()
        {
            if (!File.Exists(Filename)) return;

            var json = File.ReadAllText(Filename);

            var _config = JsonConvert.DeserializeObject<Config>(json);

            if (_config == null)
            {
                Console.WriteLine($"ConfigManager.LoadConfig() - failed to parse {Filename}");
                return;
            }
            config = _config;
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
