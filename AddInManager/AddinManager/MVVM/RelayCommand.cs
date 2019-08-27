using System;
using System.Collections.Generic;
using System.Text;

namespace AddinManager.MVVM
{
    /// <summary>
    /// 不带参数通用命令类.
    /// </summary>
    /// <seealso cref="System.Windows.Input.ICommand" />
    class RelayCommand : System.Windows.Input.ICommand
    {
        #region fields
        private readonly Action m_Execute;
        private readonly Func<bool> m_CanExecute;
        #endregion

        #region properties
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (m_CanExecute != null)
                    System.Windows.Input.CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (m_CanExecute != null)
                    System.Windows.Input.CommandManager.RequerySuggested -= value;
            }
        }
        #endregion

        #region ctors
        public RelayCommand(Action execute)
            : this(execute, null)
        {

        }

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            var flag = execute == null;
            if (flag)
                throw new ArgumentNullException("execute");

            m_Execute = execute;
            m_CanExecute = canExecute;
        }
        #endregion

        #region methods
        [System.Diagnostics.DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return m_CanExecute == null || m_CanExecute();
        }

        public void Execute(object parameter)
        {
            m_Execute();
        }
        #endregion
    }
}
