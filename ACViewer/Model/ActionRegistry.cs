using System.Collections.Generic;
using System.Linq;

namespace ACViewer.Model
{
    public class ActionRegistry
    {
        private static ActionRegistry _instance;
        public static ActionRegistry Instance => _instance ??= new ActionRegistry();

        private readonly Dictionary<string, UserAction> _actions = new();
        private readonly Dictionary<string, List<string>> _categories = new();

        public void RegisterAction(UserAction action)
        {
            _actions[action.Name] = action;
            
            if (!_categories.ContainsKey(action.Category))
                _categories[action.Category] = new List<string>();
                
            _categories[action.Category].Add(action.Name);
        }

        public UserAction GetAction(string name)
        {
            return _actions.TryGetValue(name, out var action) ? action : null;
        }

        public IReadOnlyList<string> GetCategories()
        {
            return _categories.Keys.ToList();
        }

        public IReadOnlyList<UserAction> GetActionsInCategory(string category)
        {
            if (!_categories.ContainsKey(category))
                return new List<UserAction>();

            return _categories[category]
                .Select(name => _actions[name])
                .ToList();
        }

        public void ExecuteAction(string name)
        {
            if (_actions.TryGetValue(name, out var action) && action.IsEnabled)
                action.ExecuteAction?.Invoke();
        }

        public void Clear()
        {
            _actions.Clear();
            _categories.Clear();
        }
    }
}