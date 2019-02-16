using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mago
{
    public class RelayParameterizedCommand : ICommand
    {
        #region Private Members

        private Action<object> _Action;

        #endregion

        public event EventHandler CanExecuteChanged = (sender, e) => { };

        public RelayParameterizedCommand(Action<object> action)
        {
            _Action = action;
        }

        public bool CanExecute(object parameter) { return true; }

        public void Execute(object parameter)
        {
            _Action(parameter);
        }
    }
}
