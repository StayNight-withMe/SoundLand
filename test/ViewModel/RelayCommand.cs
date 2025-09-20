using Microsoft.Windows.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace test.ViewModel
{
    internal class RelayCommand<T>: ICommand
    {

        private Action<T> _execute;
        private Func<T, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove {  CommandManager.RequerySuggested -= value; }

        }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null) 
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? true;

        public void Execute(object parameter)  => _execute((T)parameter);
    }
}
