using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using ACViewer.Entity;

namespace ACViewer.Config
{
    public class KeyBindingConfig
    {
        // Camera Controls
        public GameKeyBinding MoveForward { get; set; }
        public GameKeyBinding MoveBackward { get; set; }
        public GameKeyBinding StrafeLeft { get; set; }
        public GameKeyBinding StrafeRight { get; set; }
        public GameKeyBinding MoveUp { get; set; }
        public GameKeyBinding MoveDown { get; set; }
        
        // Z-Level Controls
        public GameKeyBinding ToggleZLevel { get; set; }
        public GameKeyBinding IncreaseZLevel { get; set; }
        public GameKeyBinding DecreaseZLevel { get; set; }

        // Custom bindings for extensibility
        public Dictionary<string, GameKeyBinding> CustomBindings { get; set; }
        private readonly Dictionary<string, GameKeyBinding> _actionBindings = new();

        public KeyBindingConfig()
        {
            CustomBindings = new Dictionary<string, GameKeyBinding>();
            SetDefaults();
        }

        public void SetDefaults()
        {
            // Camera controls
            MoveForward = new GameKeyBinding(Keys.W, ModifierKeys.None, "Move Forward", "Camera");
            MoveBackward = new GameKeyBinding(Keys.S, ModifierKeys.None, "Move Backward", "Camera");
            StrafeLeft = new GameKeyBinding(Keys.A, ModifierKeys.None, "Strafe Left", "Camera");
            StrafeRight = new GameKeyBinding(Keys.D, ModifierKeys.None, "Strafe Right", "Camera");
            MoveUp = new GameKeyBinding(Keys.Space, ModifierKeys.None, "Move Up", "Camera");
            MoveDown = new GameKeyBinding(Keys.LeftShift, ModifierKeys.None, "Move Down", "Camera");

            // Z-Level controls
            ToggleZLevel = new GameKeyBinding(Keys.F3, ModifierKeys.None, "Toggle Z-Level", "Z-Level");
            IncreaseZLevel = new GameKeyBinding(Keys.OemPlus, ModifierKeys.Alt, "Increase Z-Level", "Z-Level");
            DecreaseZLevel = new GameKeyBinding(Keys.OemMinus, ModifierKeys.Alt, "Decrease Z-Level", "Z-Level");
        }

        public void ValidateConfig()
        {
            // Ensure no null bindings
            if (MoveForward == null) MoveForward = new GameKeyBinding();
            if (MoveBackward == null) MoveBackward = new GameKeyBinding();
            if (StrafeLeft == null) StrafeLeft = new GameKeyBinding();
            if (StrafeRight == null) StrafeRight = new GameKeyBinding();
            if (MoveUp == null) MoveUp = new GameKeyBinding();
            if (MoveDown == null) MoveDown = new GameKeyBinding();
            if (ToggleZLevel == null) ToggleZLevel = new GameKeyBinding();
            if (IncreaseZLevel == null) IncreaseZLevel = new GameKeyBinding();
            if (DecreaseZLevel == null) DecreaseZLevel = new GameKeyBinding();
            if (CustomBindings == null) CustomBindings = new Dictionary<string, GameKeyBinding>();
        }

        public GameKeyBinding GetBindingForAction(string actionName)
        {
            switch (actionName)
            {
                case "Move Forward": return MoveForward;
                case "Move Backward": return MoveBackward;
                case "Strafe Left": return StrafeLeft;
                case "Strafe Right": return StrafeRight;
                case "Move Up": return MoveUp;
                case "Move Down": return MoveDown;
                case "Toggle Z-Level": return ToggleZLevel;
                case "Increase Z-Level": return IncreaseZLevel;
                case "Decrease Z-Level": return DecreaseZLevel;
            }

            if (_actionBindings.TryGetValue(actionName, out var binding))
                return binding;

            return new GameKeyBinding();
        }

        public void SetBindingForAction(string actionName, GameKeyBinding binding)
{
    System.Diagnostics.Debug.WriteLine($"SetBindingForAction - Name: {actionName}, Key: {binding.MainKey}, Modifiers: {binding.Modifiers}");
    
    switch (actionName)
    {
        case "Move Forward": 
            MoveForward = binding;
            System.Diagnostics.Debug.WriteLine($"Set MoveForward to {binding.MainKey}");
            break;
        case "Move Backward": 
            MoveBackward = binding;
            System.Diagnostics.Debug.WriteLine($"Set MoveBackward to {binding.MainKey}");
            break;
        case "Strafe Left": 
            StrafeLeft = binding;
            System.Diagnostics.Debug.WriteLine($"Set StrafeLeft to {binding.MainKey}");
            break;
        case "Strafe Right": 
            StrafeRight = binding;
            System.Diagnostics.Debug.WriteLine($"Set StrafeRight to {binding.MainKey}");
            break;
        case "Move Up": 
            MoveUp = binding;
            System.Diagnostics.Debug.WriteLine($"Set MoveUp to {binding.MainKey}");
            break;
        case "Move Down": 
            MoveDown = binding;
            System.Diagnostics.Debug.WriteLine($"Set MoveDown to {binding.MainKey}");
            break;
        case "Toggle Z-Level": 
            ToggleZLevel = binding;
            System.Diagnostics.Debug.WriteLine($"Set ToggleZLevel to {binding.MainKey}");
            break;
        case "Increase Z-Level": 
            IncreaseZLevel = binding;
            System.Diagnostics.Debug.WriteLine($"Set IncreaseZLevel to {binding.MainKey}");
            break;
        case "Decrease Z-Level": 
            DecreaseZLevel = binding;
            System.Diagnostics.Debug.WriteLine($"Set DecreaseZLevel to {binding.MainKey}");
            break;
        default:
            CustomBindings[actionName] = binding;
            System.Diagnostics.Debug.WriteLine($"Set CustomBinding {actionName} to {binding.MainKey}");
            break;
    }
}

        public void ExportToFile(string filePath)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.Auto
                };

                var json = JsonConvert.SerializeObject(this, settings);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to export keybindings", ex);
            }
        }

        public static KeyBindingConfig ImportFromFile(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };

                var config = JsonConvert.DeserializeObject<KeyBindingConfig>(json, settings);
                config.ValidateConfig();
                return config;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to import keybindings", ex);
            }
        }

        public static KeyBindingConfig CreateProfile(string profileType)
        {
            var config = new KeyBindingConfig();

            switch (profileType.ToLower())
            {
                case "default":
                    // Already handled by constructor
                    break;

                case "alternative":
                    // Alternative ESDF layout
                    config.MoveForward = new GameKeyBinding(Keys.E, ModifierKeys.None, "Move Forward", "Camera");
                    config.MoveBackward = new GameKeyBinding(Keys.D, ModifierKeys.None, "Move Backward", "Camera");
                    config.StrafeLeft = new GameKeyBinding(Keys.S, ModifierKeys.None, "Strafe Left", "Camera");
                    config.StrafeRight = new GameKeyBinding(Keys.F, ModifierKeys.None, "Strafe Right", "Camera");
                    config.MoveUp = new GameKeyBinding(Keys.Space, ModifierKeys.None, "Move Up", "Camera");
                    config.MoveDown = new GameKeyBinding(Keys.LeftControl, ModifierKeys.None, "Move Down", "Camera");
                    break;

                case "arrows":
                    // Arrow key layout
                    config.MoveForward = new GameKeyBinding(Keys.Up, ModifierKeys.None, "Move Forward", "Camera");
                    config.MoveBackward = new GameKeyBinding(Keys.Down, ModifierKeys.None, "Move Backward", "Camera");
                    config.StrafeLeft = new GameKeyBinding(Keys.Left, ModifierKeys.None, "Strafe Left", "Camera");
                    config.StrafeRight = new GameKeyBinding(Keys.Right, ModifierKeys.None, "Strafe Right", "Camera");
                    config.MoveUp = new GameKeyBinding(Keys.PageUp, ModifierKeys.None, "Move Up", "Camera");
                    config.MoveDown = new GameKeyBinding(Keys.PageDown, ModifierKeys.None, "Move Down", "Camera");
                    break;

                default:
                    throw new ArgumentException($"Unknown profile type: {profileType}");
            }

            return config;
        }
    }
}