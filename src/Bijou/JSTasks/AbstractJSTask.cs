using System;
using Bijou.Chakra;
using Bijou.Types;
using FluentResults;

namespace Bijou.JSTasks
{
    internal abstract class AbstractJSTask
    {
        public DateTime ScheduledTime { get; private set; }

        public int ScheduledDelay { get; }

        public bool IsReadyForExecution => MillisecondsToExecution == 0;

        public int MillisecondsToExecution => Math.Max((int)(ScheduledTime - DateTime.UtcNow).TotalMilliseconds, 0);

        public bool ShouldReschedule { get; private set; }

        public bool IsCanceled { get; private set; }

        public bool IsPromise { get; set; } = false;

        public Guid Id { get; }

        protected AbstractJSTask(int delay = 0, bool shouldReschedule = false)
        {
            ScheduledDelay = delay;
            ShouldReschedule = shouldReschedule;
            ResetScheduledTime();
            IsCanceled = false;
            Id = Guid.NewGuid();
        }

        public void ResetScheduledTime()
        {
            ScheduledTime = DateTime.UtcNow.AddMilliseconds(ScheduledDelay);
        }

        public void Cancel()
        {
            ShouldReschedule = false;
            IsCanceled = true;
        }

        public Result<JavaScriptObject> Execute()
        {
            if (!JavaScriptContext.IsCurrentValid)
            {
                return Results.Fail("AbstractJSTask.Execute invalid context");
            }

            // Skip execution if task is canceled
            var ret = Results.Ok(JavaScriptObject.Invalid);
            if (!IsCanceled)
            {
                ret = ExecuteImpl();
            }

            // if not rescheduled, release resources
            // can't be done from class destructor as 
            // this needs to be called from JS Context thread
            if (!ShouldReschedule) 
            {
                ReleaseJsResources();
            }

            return ret;
        }

        /// <summary>
        /// Execute task implementation
        /// </summary>
        /// <returns></returns>
        protected abstract Result<JavaScriptObject> ExecuteImpl();

        /// <summary>
        /// Release allocated JS resources
        /// </summary>
        protected virtual void ReleaseJsResources() { }
    }
}
