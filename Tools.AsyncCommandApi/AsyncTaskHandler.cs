using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.AsyncCommandApi
{
    public sealed class AsyncTaskHandler<TResult> : INotifyPropertyChanged
    {
        #region Properties
        public Task<TResult> Task { get; private set; }
        public TResult Result { get { return Task.Status == TaskStatus.RanToCompletion ? Task.Result : default; } }
        public TaskStatus Status { get { return Task.Status; } }

        public bool IsCompleted { get { return Task.IsCompleted; } }
        public bool IsNotCompleted { get { return !IsCompleted; } }

        public bool IsSuccessful { get { return Task.Status == TaskStatus.RanToCompletion; } }
        public bool IsCancelled { get { return Task.IsCanceled; } }
        public bool IsFaulted { get { return Task.IsFaulted; } }

        public AggregateException Exception { get { return Task.Exception; } }
        public Exception InnerException { get { return Exception?.InnerException; } }
        public string ErrorMessage { get { return InnerException?.Message; } }

        public Task TaskCompletion { get; private set; }
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructor
        public AsyncTaskHandler(Task<TResult> _task)
        {
            Task = _task;

            if (!_task.IsCompleted)
                TaskCompletion = WatchAsyncTask(_task);
        }
        #endregion

        #region Methods
        private void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private async Task WatchAsyncTask(Task task)
        {
            try
            {
                await task;
            }
            catch
            {
                RaisePropertyChanged("Task");
            }

            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if (propertyChanged == null)
                return;

            RaisePropertyChanged("Status");
            RaisePropertyChanged("IsCompleted");
            RaisePropertyChanged("IsNotCompleted");

            if (task.IsCanceled)
                RaisePropertyChanged("IsCancelled");
            else if (task.IsFaulted)
            {
                RaisePropertyChanged("IsFaulted");
                RaisePropertyChanged("Exception");
                RaisePropertyChanged("InnerException");
                RaisePropertyChanged("ErrorMessage");
            }
            else
            {
                RaisePropertyChanged("IsSuccessful");
                RaisePropertyChanged("Result");
            }
        }
        #endregion
    }
}