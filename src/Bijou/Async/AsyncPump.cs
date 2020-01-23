////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Original from http://blogs.msdn.com/b/pfxteam/archive/2012/01/20/10259049.aspx
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bijou.Async
{
    /// <summary>Provides a pump that supports running asynchronous methods on the current thread. </summary>
    public static class AsyncPump
    {
        /// <summary>Runs the specified asynchronous function.</summary>
        /// <param name="func">The asynchronous function to execute.</param>
        public static Task Run(Func<Task> func)
        {
            return Task.Run(() => {
                if (func == null) throw new ArgumentNullException(nameof(func));

                var prevCtx = SynchronizationContext.Current;
                try {
                    // Establish the new context
                    var syncCtx = new SingleThreadSynchronizationContext();
                    SynchronizationContext.SetSynchronizationContext(syncCtx);

                    // Invoke the function and alert the context to when it completes
                    var t = func();
                    if (t == null) throw new InvalidOperationException("No task provided.");
                    t.ContinueWith(delegate { syncCtx.Complete(); }, TaskScheduler.Default);

                    // Pump continuations and propagate any exceptions
                    syncCtx.RunOnCurrentThread();
                    t.GetAwaiter().GetResult();
                } finally { SynchronizationContext.SetSynchronizationContext(prevCtx); }
            });
        }

        /// <summary>Provides a SynchronizationContext that's single-threaded.</summary>
        private sealed class SingleThreadSynchronizationContext : SynchronizationContext, IDisposable
        {
            /// <summary>The queue of work items.</summary>
            private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> m_queue =
                new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();
            /// <summary>Track whether Dispose has been called.</summary>
            bool _isDisposed = false;

            /// <summary>Destructs the instance of SingleThreadSynchronizationContext</summary>
            ~SingleThreadSynchronizationContext()
            {
                Dispose(false);
            }

            /// <summary>Dispatches an asynchronous message to the synchronization context.</summary>
            /// <param name="d">The System.Threading.SendOrPostCallback delegate to call.</param>
            /// <param name="state">The object passed to the delegate.</param>
            public override void Post(SendOrPostCallback d, object state)
            {
                if (d == null) throw new ArgumentNullException(nameof(d));

                // WINDOWS-4179 : Possible race condition here when disposing of the BackendModule used.
                // This does the job but we should have a better understanding of the root cause
                if (!m_queue.IsAddingCompleted) {
                    m_queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
                }
            }

            /// <summary>Not supported.</summary>
            public override void Send(SendOrPostCallback d, object state)
            {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                throw new NotSupportedException("Synchronously sending is not supported.");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
            }

            /// <summary>Runs an loop to process all queued work items.</summary>
            public void RunOnCurrentThread()
            {
                foreach (var workItem in m_queue.GetConsumingEnumerable())
                    workItem.Key(workItem.Value);
            }

            /// <summary>Notifies the context that no more work will arrive.</summary>
            public void Complete() { m_queue.CompleteAdding(); }


            /// <summary>
            /// Dispose(bool disposing) executes in two distinct scenarios.
            /// If disposing equals true, the method has been called directly
            /// or indirectly by a user's code. Managed and unmanaged resources
            /// can be disposed.
            /// If disposing equals false, the method has been called by the
            /// runtime from inside the finalizer and you should not reference
            /// other objects. Only unmanaged resources can be disposed.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                // This object will be cleaned up by the Dispose method.
                // Therefore, you should call GC.SupressFinalize to
                // take this object off the finalization queue
                // and prevent finalization code for this object
                // from executing a second time.
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Dispose(bool disposing) executes in two distinct scenarios.
            /// If disposing equals true, the method has been called directly
            /// or indirectly by a user's code. Managed and unmanaged resources
            /// can be disposed.
            /// If disposing equals false, the method has been called by the
            /// runtime from inside the finalizer and you should not reference
            /// other objects. Only unmanaged resources can be disposed.
            /// </summary>
            private void Dispose(bool disposing)
            {
                if (!_isDisposed) {
                    if (disposing) {
                        m_queue.Dispose();
                    }

                    _isDisposed = true;
                }
            }
        }
    }
}
