using System;
using System.Diagnostics;
using Bijou.Chakra.Hosting;

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

        public int Id { get; set; } = -1;

        public bool HasValidId => Id != -1;

        protected AbstractJSTask(int delay = 0, bool shouldReschedule = false)
        {
            ScheduledDelay = delay;
            ShouldReschedule = shouldReschedule;
            ResetScheduledTime();
            IsCanceled = false;
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

        public JavaScriptValue Execute()
        {
            var ret = JavaScriptValue.Invalid;

            // Ensure there is a valid context
            if (!JavaScriptContext.IsCurrentValid) 
            {
                Debug.WriteLine("JSTaskAbstract.Execute invalid context");
                return ret;
            }

            // Skip execution if task is canceled
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
        protected abstract JavaScriptValue ExecuteImpl();

        /// <summary>
        /// Release allocated JS resources
        /// </summary>
        protected virtual void ReleaseJsResources() { }
    }
}
