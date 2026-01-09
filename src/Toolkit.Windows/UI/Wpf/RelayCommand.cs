using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Xarial.XCad.Toolkit.Windows.UI.Wpf
{
    internal class RelayCommand : ICommand
    {
        private readonly Action m_Execute;

        public RelayCommand(Action execute)
        {
            m_Execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => m_Execute.Invoke();

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
    }
}
