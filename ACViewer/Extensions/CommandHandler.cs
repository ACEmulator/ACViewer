using System;
using System.Windows.Input;

namespace ACViewer.Extensions
{
    public class CommandHandler : ICommand
    {
        private Action<object> _action;
        private Func<object, bool> _canExecute;

        /// <summary>
        /// Creates instance of the command handler
        /// </summary>
        /// <param name="action">Action to be executed by the command</param>
        /// <param name="canExecute">A bolean property to containing current permissions to execute the command</param>
        public CommandHandler(Action<object> action, Func<object, bool> canExecute)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            _action = action;
            _canExecute = canExecute ?? (x => true);
        }
        public CommandHandler(Action<object> action) : this(action, null)
        {
        }

        /// <summary>
        /// Wires CanExecuteChanged event 
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Forcess checking if execute is allowed
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }
        public void Refresh()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
