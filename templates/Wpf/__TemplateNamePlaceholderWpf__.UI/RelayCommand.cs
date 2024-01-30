using System;
using System.Windows.Input;

namespace __TemplateNamePlaceholderWpf__.UI
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        private readonly Predicate<object> m_CanExecuteFunc;
        private readonly Action<object> m_ExecuteFunc;

        public RelayCommand(Action<object> executeFunc, Predicate<object> canExecuteFunc)
        {
            m_CanExecuteFunc = canExecuteFunc;
            m_ExecuteFunc = executeFunc;
        }

        public bool CanExecute(object parameter) => m_CanExecuteFunc.Invoke(parameter);
        public void Execute(object parameter) => m_ExecuteFunc.Invoke(parameter);
    }
}
