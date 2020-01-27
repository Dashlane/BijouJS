using System;
using Bijou.JSTasks;
using Bijou.Types;
using FluentResults;

namespace Bijou
{
    internal struct EventLoopTask
    {
        private readonly Action<Result<JavaScriptObject>> _completeHandler;

        public AbstractJSTask Task { get; }

        public EventLoopTask(
            AbstractJSTask task, 
            Action<Result<JavaScriptObject>> completeHandler = null)
        {

            _completeHandler = completeHandler;
            Task = task;
        }

        public void CompleteTask(Result<JavaScriptObject> value)
        {
            _completeHandler?.Invoke(value);
        }
    }
}