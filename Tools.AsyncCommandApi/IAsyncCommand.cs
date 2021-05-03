using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tools.AsyncCommandApi
{
    interface IAsyncCommand : ICommand 
    {
        Task ExecuteAsync(object parameter);
    }
}
