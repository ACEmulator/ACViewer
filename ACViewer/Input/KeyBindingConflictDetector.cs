using System.Collections.Generic;
using System.Linq;
using ACViewer.Config;
using ACViewer.Entity;

namespace ACViewer.Input
{
    public class KeyBindingConflictDetector
    {
        public struct Conflict
        {
            public string Action1 { get; set; }
            public string Action2 { get; set; }
            public string DisplayString { get; set; }

            public Conflict(string action1, string action2, string displayString)
            {
                Action1 = action1;
                Action2 = action2;
                DisplayString = displayString;
            }
        }

        public static List<Conflict> DetectConflicts(KeyBindingConfig config)
        {
            var conflicts = new List<Conflict>();
            var allBindings = new List<(string Action, GameKeyBinding Binding)>();

            // Collect all bindings
            AddBinding("Move Forward", config.MoveForward);
            AddBinding("Move Backward", config.MoveBackward);
            AddBinding("Strafe Left", config.StrafeLeft);
            AddBinding("Strafe Right", config.StrafeRight);
            AddBinding("Move Up", config.MoveUp);
            AddBinding("Move Down", config.MoveDown);
            AddBinding("Toggle Z-Level", config.ToggleZLevel);
            AddBinding("Increase Z-Level", config.IncreaseZLevel);
            AddBinding("Decrease Z-Level", config.DecreaseZLevel);

            foreach (var customBinding in config.CustomBindings)
                AddBinding(customBinding.Key, customBinding.Value);

            // Check for conflicts
            for (int i = 0; i < allBindings.Count; i++)
            {
                for (int j = i + 1; j < allBindings.Count; j++)
                {
                    var binding1 = allBindings[i];
                    var binding2 = allBindings[j];

                    if (!binding1.Binding.IsEmpty && !binding2.Binding.IsEmpty &&
                        binding1.Binding.MainKey == binding2.Binding.MainKey &&
                        binding1.Binding.Modifiers == binding2.Binding.Modifiers)
                    {
                        conflicts.Add(new Conflict(
                            binding1.Action,
                            binding2.Action,
                            binding1.Binding.GetDisplayString()
                        ));
                    }
                }
            }

            return conflicts;

            void AddBinding(string action, GameKeyBinding binding)
            {
                if (binding != null)
                    allBindings.Add((action, binding));
            }
        }
    }
}