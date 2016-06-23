using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CircuitMVVMBase.Commands
{
    public class RelayCommand : ICommand
    {
        readonly Action<object> _execute;
        protected Func<object, Task> _asyncExecute;
        Predicate<object> _canExecute;

        public Predicate<object> CanExecuteTarget
        {
            get { return _canExecute; }
            set { _canExecute = value; }
        }

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        { }

        public RelayCommand(Func<object, Task> execute)
        {
            _asyncExecute = execute;
        }

        public RelayCommand(Action<object> execute,
                            Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            if (_execute != null)
                _execute(parameter);
            else _asyncExecute?.Invoke(parameter);
        }

    }
}
