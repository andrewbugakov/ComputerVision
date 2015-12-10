using System;
using System.Windows.Input;

namespace VideoFeatureMatching.Utils
{
    public class Command<T> : ICommand
    {
        private readonly Action<T> _action;
        private readonly Func<T, bool> _canExecute;

        public Command(Action<T> action)
            : this(action, parametr => true)
        { }

        public Command(Action<T> action, Func<T, bool> canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public Command(Action<T> action, Func<bool> canExecute)
            : this(action, arg => canExecute())
        {
            
        }

        public void Execute(T parametr)
        {
            if (CanExecute(parametr))
            {
                _action(parametr);
            }
        }

        public bool CanExecute(T parametr)
        {
            return _canExecute.Invoke(parametr);
        }

        bool ICommand.CanExecute(object parametr)
        {
            return CanExecute((T)parametr);
        }

        void ICommand.Execute(object parametr)
        {
            Execute((T)parametr);
        }

        public event EventHandler CanExecuteChanged;
    }
}