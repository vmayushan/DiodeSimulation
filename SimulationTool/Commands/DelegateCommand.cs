using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace SimulationTool.Commands
{
    /// <summary>
    ///     This class allows delegating the commanding logic to methods passed as parameters,
    ///     and enables a View to bind commands to objects that are not part of the element tree.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        #region Constructors

        /// <summary>
        ///     Constructor
        /// </summary>
        public DelegateCommand(Action executeMethod)
            : this(executeMethod, null, false)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
            : this(executeMethod, canExecuteMethod, false)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod, bool isAutomaticRequeryDisabled)
        {
            if (executeMethod == null)
            {
                throw new ArgumentNullException(nameof(executeMethod));
            }

            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
            this.isAutomaticRequeryDisabled = isAutomaticRequeryDisabled;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Method to determine if the command can be executed
        /// </summary>
        public bool CanExecute()
        {
            if (canExecuteMethod != null)
            {
                return canExecuteMethod();
            }
            return true;
        }

        /// <summary>
        ///     Execution of the command
        /// </summary>
        public void Execute()
        {
            executeMethod?.Invoke();
        }

        /// <summary>
        ///     Property to enable or disable CommandManager's automatic requery on this command
        /// </summary>
        public bool IsAutomaticRequeryDisabled
        {
            get { return isAutomaticRequeryDisabled; }
            set
            {
                if (isAutomaticRequeryDisabled == value) return;
                if (value)
                {
                    CommandManagerHelper.RemoveHandlersFromRequerySuggested(canExecuteChangedHandlers);
                }
                else
                {
                    CommandManagerHelper.AddHandlersToRequerySuggested(canExecuteChangedHandlers);
                }
                isAutomaticRequeryDisabled = value;
            }
        }

        /// <summary>
        ///     Raises the CanExecuteChaged event
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

        /// <summary>
        ///     Protected virtual method to raise CanExecuteChanged event
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
            CommandManagerHelper.CallWeakReferenceHandlers(canExecuteChangedHandlers);
        }

        #endregion

        #region ICommand Members

        /// <summary>
        ///     ICommand.CanExecuteChanged implementation
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (!isAutomaticRequeryDisabled)
                {
                    CommandManager.RequerySuggested += value;
                }
                CommandManagerHelper.AddWeakReferenceHandler(ref canExecuteChangedHandlers, value, 2);
            }
            remove
            {
                if (!isAutomaticRequeryDisabled)
                {
                    CommandManager.RequerySuggested -= value;
                }
                CommandManagerHelper.RemoveWeakReferenceHandler(canExecuteChangedHandlers, value);
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute();
        }

        void ICommand.Execute(object parameter)
        {
            Execute();
        }

        #endregion

        #region Data

        private readonly Action executeMethod;
        private readonly Func<bool> canExecuteMethod;
        private bool isAutomaticRequeryDisabled;
        private List<WeakReference> canExecuteChangedHandlers;

        #endregion
    }

    /// <summary>
    ///     This class allows delegating the commanding logic to methods passed as parameters,
    ///     and enables a View to bind commands to objects that are not part of the element tree.
    /// </summary>
    /// <typeparam name="T">Type of the parameter passed to the delegates</typeparam>
    public class DelegateCommand<T> : ICommand
    {
        #region Constructors

        /// <summary>
        ///     Constructor
        /// </summary>
        public DelegateCommand(Action<T> executeMethod)
            : this(executeMethod, null, false)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
            : this(executeMethod, canExecuteMethod, false)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod, bool isAutomaticRequeryDisabled)
        {
            if (executeMethod == null)
            {
                throw new ArgumentNullException("executeMethod");
            }

            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
            this.isAutomaticRequeryDisabled = isAutomaticRequeryDisabled;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Method to determine if the command can be executed
        /// </summary>
        public bool CanExecute(T parameter)
        {
            if (canExecuteMethod != null)
            {
                return canExecuteMethod(parameter);
            }
            return true;
        }

        /// <summary>
        ///     Execution of the command
        /// </summary>
        public void Execute(T parameter)
        {
            if (executeMethod != null)
            {
                executeMethod(parameter);
            }
        }

        /// <summary>
        ///     Raises the CanExecuteChaged event
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

        /// <summary>
        ///     Protected virtual method to raise CanExecuteChanged event
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
            CommandManagerHelper.CallWeakReferenceHandlers(canExecuteChangedHandlers);
        }

        /// <summary>
        ///     Property to enable or disable CommandManager's automatic requery on this command
        /// </summary>
        public bool IsAutomaticRequeryDisabled
        {
            get { return isAutomaticRequeryDisabled; }
            set
            {
                if (isAutomaticRequeryDisabled != value)
                {
                    if (value)
                    {
                        CommandManagerHelper.RemoveHandlersFromRequerySuggested(canExecuteChangedHandlers);
                    }
                    else
                    {
                        CommandManagerHelper.AddHandlersToRequerySuggested(canExecuteChangedHandlers);
                    }
                    isAutomaticRequeryDisabled = value;
                }
            }
        }

        #endregion

        #region ICommand Members

        /// <summary>
        ///     ICommand.CanExecuteChanged implementation
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (!isAutomaticRequeryDisabled)
                {
                    CommandManager.RequerySuggested += value;
                }
                CommandManagerHelper.AddWeakReferenceHandler(ref canExecuteChangedHandlers, value, 2);
            }
            remove
            {
                if (!isAutomaticRequeryDisabled)
                {
                    CommandManager.RequerySuggested -= value;
                }
                CommandManagerHelper.RemoveWeakReferenceHandler(canExecuteChangedHandlers, value);
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            // if T is of value type and the parameter is not
            // set yet, then return false if CanExecute delegate
            // exists, else return true
            if (parameter == null &&
                typeof (T).IsValueType)
            {
                return canExecuteMethod == null;
            }
            return CanExecute((T) parameter);
        }

        void ICommand.Execute(object parameter)
        {
            Execute((T) parameter);
        }

        #endregion

        #region Data

        private readonly Action<T> executeMethod;
        private readonly Func<T, bool> canExecuteMethod;
        private bool isAutomaticRequeryDisabled;
        private List<WeakReference> canExecuteChangedHandlers;

        #endregion
    }

    /// <summary>
    ///     This class contains methods for the CommandManager that help avoid memory leaks by
    ///     using weak references.
    /// </summary>
    internal class CommandManagerHelper
    {
        internal static void CallWeakReferenceHandlers(List<WeakReference> handlers)
        {
            if (handlers != null)
            {
                // Take a snapshot of the handlers before we call out to them since the handlers
                // could cause the array to me modified while we are reading it.

                var callees = new EventHandler[handlers.Count];
                var count = 0;

                for (var i = handlers.Count - 1; i >= 0; i--)
                {
                    var reference = handlers[i];
                    var handler = reference.Target as EventHandler;
                    if (handler == null)
                    {
                        // Clean up old handlers that have been collected
                        handlers.RemoveAt(i);
                    }
                    else
                    {
                        callees[count] = handler;
                        count++;
                    }
                }

                // Call the handlers that we snapshotted
                for (var i = 0; i < count; i++)
                {
                    var handler = callees[i];
                    handler(null, EventArgs.Empty);
                }
            }
        }

        internal static void AddHandlersToRequerySuggested(List<WeakReference> handlers)
        {
            if (handlers != null)
            {
                foreach (var handlerRef in handlers)
                {
                    var handler = handlerRef.Target as EventHandler;
                    if (handler != null)
                    {
                        CommandManager.RequerySuggested += handler;
                    }
                }
            }
        }

        internal static void RemoveHandlersFromRequerySuggested(List<WeakReference> handlers)
        {
            if (handlers != null)
            {
                foreach (var handlerRef in handlers)
                {
                    var handler = handlerRef.Target as EventHandler;
                    if (handler != null)
                    {
                        CommandManager.RequerySuggested -= handler;
                    }
                }
            }
        }

        internal static void AddWeakReferenceHandler(ref List<WeakReference> handlers, EventHandler handler)
        {
            AddWeakReferenceHandler(ref handlers, handler, -1);
        }

        internal static void AddWeakReferenceHandler(ref List<WeakReference> handlers, EventHandler handler,
            int defaultListSize)
        {
            if (handlers == null)
            {
                handlers = defaultListSize > 0 ? new List<WeakReference>(defaultListSize) : new List<WeakReference>();
            }

            handlers.Add(new WeakReference(handler));
        }

        internal static void RemoveWeakReferenceHandler(List<WeakReference> handlers, EventHandler handler)
        {
            if (handlers != null)
            {
                for (var i = handlers.Count - 1; i >= 0; i--)
                {
                    var reference = handlers[i];
                    var existingHandler = reference.Target as EventHandler;
                    if ((existingHandler == null) || (existingHandler == handler))
                    {
                        // Clean up old handlers that have been collected
                        // in addition to the handler that is to be removed.
                        handlers.RemoveAt(i);
                    }
                }
            }
        }
    }
}