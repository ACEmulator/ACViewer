using Microsoft.Xna.Framework.Input;
using System;
using System.Windows.Input;

namespace ACViewer.Entity
{
    public class GameKeyBinding
    {
        public Keys MainKey { get; set; }
        public ModifierKeys Modifiers { get; set; }
        public string DisplayName { get; set; }
        public string Category { get; set; }

        public GameKeyBinding(Keys mainKey = Keys.None, ModifierKeys modifiers = ModifierKeys.None, string displayName = "", string category = "")
        {
            MainKey = mainKey;
            Modifiers = modifiers;
            DisplayName = displayName;
            Category = category;
        }

        public bool IsEmpty => MainKey == Keys.None;

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

        public GameKeyBinding Clone()
        {
            return new GameKeyBinding(MainKey, Modifiers, DisplayName, Category);
        }
    }
}