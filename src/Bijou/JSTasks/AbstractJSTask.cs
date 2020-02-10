using System;
using Bijou.Chakra;
using FluentResults;

namespace Bijou.JSTasks
{
    public abstract class AbstractJSTask
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

        /// <summary>
        /// Resets the scheduled time for execution based on the current clock time.
        /// </summary>
        public void ResetScheduledTime()
        {
            ScheduledTime = DateTime.UtcNow.AddMilliseconds(ScheduledDelay);
        }

        /// <summary>
        /// Cancels the task.
        /// </summary>
        public void Cancel()
        {
            ShouldReschedule = false;
            IsCanceled = true;
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        public Result<JavaScriptValue> Execute()
        {
            if (!JavaScriptContext.IsCurrentValid)
            {
                return Results.Fail("AbstractJSTask.Execute invalid context");
            }

            // Skip execution if task is canceled.
            var ret = Results.Ok(JavaScriptValue.Invalid);
            if (!IsCanceled)
            {
                ret = ExecuteImpl();
            }

            // If not rescheduled, release resources.
            // It can't be done from class destructor as this needs to be called from JS Context thread.
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
        protected abstract Result<JavaScriptValue> ExecuteImpl();

        /// <summary>
        /// Release allocated JS resources
        /// </summary>
        protected virtual void ReleaseJsResources() { }
    }
}
