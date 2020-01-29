using System;
using Bijou.JSTasks;
using FluentResults;

namespace Bijou
{
    internal struct EventLoopTask
    {
        private readonly Action<Result> _completeHandler;

        public AbstractJSTask Task { get; }

        public EventLoopTask(
            AbstractJSTask task, 
            Action<Result> completeHandler = null)
        {

            _completeHandler = completeHandler;
            Task = task;
        }

        public void CompleteTask(Result value)
        {
            _completeHandler?.Invoke(value);
        }
    }
}