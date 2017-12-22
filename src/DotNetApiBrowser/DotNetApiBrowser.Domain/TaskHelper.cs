using System;
using System.Threading;
using System.Threading.Tasks;

namespace Waf.DotNetApiBrowser.Domain
{
    public static class TaskHelper
    {
        public static Task Run(Action action, TaskScheduler scheduler)
        {
            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.DenyChildAttach, scheduler);
        }

        public static Task<T> Run<T>(Func<T> action, TaskScheduler scheduler)
        {
            return Task<T>.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.DenyChildAttach, scheduler);
        }
    }
}
