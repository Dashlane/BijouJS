using System;
using Bijou.JSTasks;
using FluentResults;

namespace Bijou
{
    internal struct EventLoopTask
    {
        private readonly Action<Result> _completeHandler;

        public AbstractJSTask Task { get; }

        /// <summary>
        /// Constructs a task to be added to the event loop.
        /// </summary>
        /// <param name="task">JS task</param>
        /// <param name="completeHandler">Callback when task is complete</param>
        public EventLoopTask(AbstractJSTask task, Action<Result> completeHandler = null)
        {
            _completeHandler = completeHandler;
            Task = task;
        }

        /// <summary>
        /// To be called when the task is complete.
        /// </summary>
        /// <param name="value">The result of the execution of the task</param>
        public void CompleteTask(Result value)
        {
            _completeHandler?.Invoke(value);
        }
    }
}