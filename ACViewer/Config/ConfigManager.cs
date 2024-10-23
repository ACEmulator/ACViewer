// File: ACViewer/Config/ConfigManager.cs
using System;
using System.IO;
using System.Windows;
using Newtonsoft.Json;

namespace ACViewer.Config
{
    public static class ConfigManager
    {
        private static readonly string Filename = "ACViewer.json";
        private static readonly string KeybindingsFile = "keybindings.json";
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
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            var json = JsonConvert.SerializeObject(config, settings);
            File.WriteAllText(Filename, json);
        }

        public static void LoadConfig()
        {
            config = ReadConfig();
        }

        public static Config ReadConfig()
        {
            if (!File.Exists(Filename))
                return null;

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
                return config != null &&
                       config.Database != null &&
                       !string.IsNullOrWhiteSpace(config.Database.Host) &&
                       config.Database.Port > 0 &&
                       !string.IsNullOrWhiteSpace(config.Database.DatabaseName) &&
                       !string.IsNullOrWhiteSpace(config.Database.Username) &&
                       !string.IsNullOrWhiteSpace(config.Database.Password);
            }
        }

        public static bool SaveKeyBindings()
        {
            try
            {
                var json = JsonConvert.SerializeObject(Config.KeyBindingConfig, Formatting.Indented);
                File.WriteAllText(KeybindingsFile, json);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving keybindings: {ex.Message}", "Save Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static bool LoadKeyBindings()
        {
            if (!File.Exists(KeybindingsFile))
                return false;

            try
            {
                var json = File.ReadAllText(KeybindingsFile);
                var bindings = JsonConvert.DeserializeObject<KeyBindingConfig>(json);
                if (bindings != null)
                {
                    Config.KeyBindingConfig = bindings;
                    Config.KeyBindingConfig.ValidateConfig();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading keybindings: {ex.Message}", "Load Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }
    }
}