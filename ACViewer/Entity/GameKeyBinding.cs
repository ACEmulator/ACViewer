using Microsoft.Xna.Framework.Input;
using System;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ACViewer.Entity
{
    public class GameKeyBinding
    {
        [JsonProperty("MainKey")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Keys MainKey { get; set; }

        [JsonProperty("Modifiers")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ModifierKeys Modifiers { get; set; }

        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }

        [JsonProperty("Category")]
        public string Category { get; set; }

        [JsonProperty("KeyValue")]
        public int KeyValue { get; set; }

        [JsonIgnore]
        public bool IsEmpty => MainKey == Keys.None;

        public GameKeyBinding(Keys mainKey = Keys.None, ModifierKeys modifiers = ModifierKeys.None, 
            string displayName = "", string category = "")
        {
            MainKey = mainKey;
            KeyValue = (int)mainKey;  // Store the actual numeric value
            Modifiers = modifiers;
            DisplayName = displayName;
            Category = category;
        
            System.Diagnostics.Debug.WriteLine($"Creating new GameKeyBinding - Key: {mainKey}, Value: {KeyValue}");
        }

        public GameKeyBinding Clone()
        {
            System.Diagnostics.Debug.WriteLine($"Cloning GameKeyBinding - Key: {MainKey}, Value: {KeyValue}");
            var clone = new GameKeyBinding
            {
                MainKey = this.MainKey,
                KeyValue = this.KeyValue,  // Preserve the actual key value
                Modifiers = this.Modifiers,
                DisplayName = this.DisplayName,
                Category = this.Category
            };
            System.Diagnostics.Debug.WriteLine($"Cloned binding - Key: {clone.MainKey}, Value: {clone.KeyValue}");
            return clone;
        }
        
        public bool Matches(KeyboardState state, ModifierKeys currentModifiers)
        {
            if (IsEmpty) return false;
            if (!state.IsKeyDown(MainKey)) return false;
            return currentModifiers == Modifiers;
        }

        public string GetDisplayString()
        {
            if (IsEmpty) return "None";
            var modifierStr = Modifiers != ModifierKeys.None ? $"{Modifiers}+" : "";
            return $"{modifierStr}{MainKey}";
        }
    }
}