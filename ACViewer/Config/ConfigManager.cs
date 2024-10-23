// File: ACViewer/Config/ConfigManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using ACViewer.Entity;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
        
        private static JsonSerializerSettings GetSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = new JsonConverter[] 
                { 
                    new StringEnumConverter() 
                }
            };
        }

        public static Config Snapshot { get; set; }

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
                var bindings = Config.KeyBindingConfig;
                System.Diagnostics.Debug.WriteLine($"SaveKeyBindings - ToggleZLevel: {bindings.ToggleZLevel.MainKey} (Value: {bindings.ToggleZLevel.KeyValue})");

                // Create verification copy
                var verifyConfig = new KeyBindingConfig();
                CopyBindings(bindings, verifyConfig);

                var settings = GetSerializerSettings();
                var json = JsonConvert.SerializeObject(verifyConfig, settings);
        
                // Test deserialize
                var testConfig = JsonConvert.DeserializeObject<KeyBindingConfig>(json, settings);
                System.Diagnostics.Debug.WriteLine($"Test deserialize ToggleZLevel: {testConfig.ToggleZLevel.MainKey} (Value: {testConfig.ToggleZLevel.KeyValue})");

                // Verify both the key enum and the numeric value match
                if (testConfig.ToggleZLevel.MainKey != bindings.ToggleZLevel.MainKey || 
                    testConfig.ToggleZLevel.KeyValue != bindings.ToggleZLevel.KeyValue)
                {
                    throw new Exception($"Verification failed - key mismatch after serialization test." +
                                        $" Expected {bindings.ToggleZLevel.MainKey} ({bindings.ToggleZLevel.KeyValue})," +
                                        $" got {testConfig.ToggleZLevel.MainKey} ({testConfig.ToggleZLevel.KeyValue})");
                }

                File.WriteAllText(KeybindingsFile, json);
                SaveConfig();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving keybindings: {ex}");
                return false;
            }
        }

        private static void CopyBindings(KeyBindingConfig source, KeyBindingConfig target)
        {
            target.MoveForward = source.MoveForward.Clone();
            target.MoveBackward = source.MoveBackward.Clone();
            target.StrafeLeft = source.StrafeLeft.Clone();
            target.StrafeRight = source.StrafeRight.Clone();
            target.MoveUp = source.MoveUp.Clone();
            target.MoveDown = source.MoveDown.Clone();
            target.ToggleZLevel = source.ToggleZLevel.Clone();
            target.IncreaseZLevel = source.IncreaseZLevel.Clone();
            target.DecreaseZLevel = source.DecreaseZLevel.Clone();
            
            target.CustomBindings = new Dictionary<string, GameKeyBinding>();
            foreach (var kvp in source.CustomBindings)
            {
                target.CustomBindings[kvp.Key] = kvp.Value.Clone();
            }
        }

        public static void SaveConfig()
        {
            try
            {
                var settings = GetSerializerSettings();
                var json = JsonConvert.SerializeObject(Config, settings);
                File.WriteAllText(Filename, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving config: {ex}");
                throw;
            }
        }

        public static bool LoadKeyBindings()
        {
            try
            {
                if (!File.Exists(KeybindingsFile))
                {
                    Config.KeyBindingConfig = new KeyBindingConfig();
                    Config.KeyBindingConfig.SetDefaults();
                    return true;
                }

                var json = File.ReadAllText(KeybindingsFile);
                System.Diagnostics.Debug.WriteLine($"Loading keybindings: {json}");

                var settings = GetSerializerSettings();
                var bindings = JsonConvert.DeserializeObject<KeyBindingConfig>(json, settings);

                if (bindings != null)
                {
                    // Create a new config and copy values to ensure clean state
                    var newConfig = new KeyBindingConfig();
                    CopyBindings(bindings, newConfig);
                    newConfig.ValidateConfig();
                    
                    Config.KeyBindingConfig = newConfig;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading keybindings: {ex}");
                Config.KeyBindingConfig = new KeyBindingConfig();
                Config.KeyBindingConfig.SetDefaults();
                return false;
            }
        }
    }
}