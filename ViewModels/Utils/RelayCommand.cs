using System;
using System.Windows.Input;

namespace HeatLossCalc.ViewModels.Utils
{
    /// <summary>
    ///     The class contains a command called from View.
    /// </summary>
    /// <remarks>Instances of this class must be created in the ViewModel and binding in the View.</remarks>
    /// <summary>
    /// Generic Implementation of a RelayCommand.
    /// Implements the <see cref="System.Windows.Input.ICommand" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RelayCommand<T> : ICommand
    {
        #region Private Fields

        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        #endregion

        #region Constructor


        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBase{T}"/> class.
        /// </summary>
        /// <param name="execute">The execute action.</param>
        /// <param name="canExecute">The can execute predicate.</param>
        /// <exception cref="NullReferenceException">execute</exception>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new NullReferenceException(nameof(execute));
            _canExecute = canExecute ?? (_ => true);
        }

        #endregion

        #region Event Handler


        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        /// <returns><see langword="true" /> if this command can be executed; otherwise, <see langword="false" />.</returns>
        public bool CanExecute(object parameter) => _canExecute((T)parameter);


        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        public void Execute(object parameter) => _execute((T)parameter);

        #endregion

    }

    /// <summary>
    ///     The class contains a command called from View.
    /// </summary>
    /// <remarks>Instances of this class must be created in the ViewModel and binding in the View.</remarks>
    public class RelayCommand : ICommand
    {
        private readonly bool _canExecute;
        private readonly Action<object> _execute;

        public RelayCommand(Action<object> execute, bool canExecute = true)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}