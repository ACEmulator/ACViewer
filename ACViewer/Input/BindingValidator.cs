using ACViewer.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.Xna.Framework.Input;

namespace ACViewer.Input
{
    public class BindingValidator
    {
        private readonly Dictionary<string, List<GameKeyBinding>> _categoryBindings;

        public BindingValidator()
        {
            _categoryBindings = new Dictionary<string, List<GameKeyBinding>>();
        }

        public void RegisterBinding(GameKeyBinding binding)
        {
            if (!_categoryBindings.ContainsKey(binding.Category))
                _categoryBindings[binding.Category] = new List<GameKeyBinding>();

            _categoryBindings[binding.Category].Add(binding);
        }

        public void UnregisterBinding(GameKeyBinding binding)
        {
            if (_categoryBindings.ContainsKey(binding.Category))
                _categoryBindings[binding.Category].Remove(binding);
        }

        public (bool isValid, string error) ValidateBinding(GameKeyBinding newBinding)
        {
            if (newBinding.IsEmpty)
                return (true, null);

            if (IsSystemReserved(newBinding))
                return (false, $"The combination {newBinding.GetDisplayString()} is reserved by the system");

            var conflicts = FindConflicts(newBinding);
            if (conflicts.Any())
            {
                var conflictList = string.Join(", ", conflicts.Select(b => $"{b.DisplayName} ({b.GetDisplayString()})"));
                return (false, $"Conflicts with existing bindings: {conflictList}");
            }

            return (true, null);
        }

        private List<GameKeyBinding> FindConflicts(GameKeyBinding binding)
        {
            var conflicts = new List<GameKeyBinding>();

            foreach (var categoryBindings in _categoryBindings.Values)
            {
                conflicts.AddRange(categoryBindings.Where(b =>
                    b != binding &&
                    b.MainKey == binding.MainKey &&
                    b.Modifiers == binding.Modifiers));
            }

            return conflicts;
        }

        private bool IsSystemReserved(GameKeyBinding binding)
        {
            if (binding.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                if (binding.MainKey == Keys.F4 || binding.MainKey == Keys.Tab)
                    return true;
            }

            return false;
        }
    }
}
