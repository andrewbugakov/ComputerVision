using System;
using System.Windows.Input;

namespace VideoFeatureMatching.Utils
{
    public class Command : ICommand
    {
        private readonly Action _action;
        private readonly Func<bool> _canExecute;

        public Command(Action action)
            : this(action, () => true)
        { }

        public Command(Action action, Func<bool> canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public void Execute()
        {
            if (CanExecute())
            {
                _action();
            }
        }

        public bool CanExecute()
        {
            return _canExecute.Invoke();
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute();
        }

        void ICommand.Execute(object parameter)
        {
            Execute();
        }

        public event EventHandler CanExecuteChanged;
    }
}