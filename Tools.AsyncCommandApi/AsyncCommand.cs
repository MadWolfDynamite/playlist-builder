using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace Tools.AsyncCommandApi
{
    public class AsyncCommand<TResult> : AsyncCommandBase, INotifyPropertyChanged
    {
        private readonly Func<Task<TResult>> _command;

        private readonly Func<bool> _canExecute;
        private AsyncTaskHandler<TResult> _execution;

        public event PropertyChangedEventHandler PropertyChanged;

        public AsyncTaskHandler<TResult> Execution 
        { 
            get { return _execution; }
            private set { 
                if (_execution != value) 
                {
                    _execution = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Execution"));
                }
            }
        }

        public AsyncCommand(Func<Task<TResult>> command) : this(command, null) { }
        public AsyncCommand(Func<Task<TResult>> command, Func<bool> canExecute)
        {
            _command = command;
            _canExecute = canExecute;
        }

        public override bool CanExecute(object parameter)
        {
            return (Execution == null || Execution.IsCompleted) && (_canExecute == null ? true : _canExecute());
        }
        public override Task ExecuteAsync(object parameter)
        {
            Execution = new AsyncTaskHandler<TResult>(_command());
            return Execution.TaskCompletion;
        }
    }
}
