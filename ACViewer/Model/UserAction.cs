using System;

namespace ACViewer.Model
{
    public class UserAction
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public Action ExecuteAction { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; } = true;

        public UserAction(string name, string category, Action executeAction, string description = "")
        {
            Name = name;
            Category = category;
            ExecuteAction = executeAction;
            Description = description;
        }
    }
}