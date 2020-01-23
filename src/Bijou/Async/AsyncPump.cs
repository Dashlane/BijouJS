using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bijou.Async
{
    /// <summary>
    /// Provides a pump that supports running asynchronous methods on the current thread.
    /// Original from http://blogs.msdn.com/b/pfxteam/archive/2012/01/20/10259049.aspx
    /// </summary>
    public static class AsyncPump
    {
        /// <summary>
        /// Runs the specified asynchronous function.
        /// </summary>
        /// <param name="func">The asynchronous function to execute.</param>
        public static Task Run(Func<Task> func)
        {
            return Task.Run(() => {
                if (func == null) throw new ArgumentNullException(nameof(func));

                var prevSyncContext = SynchronizationContext.Current;
                try
                {
                    // Establish the new context.
                    var syncContext = new SingleThreadSynchronizationContext();
                    SynchronizationContext.SetSynchronizationContext(syncContext);

                    // Invoke the function and alert the context to when it completes.
                    var task = func();

                    if (task == null) throw new InvalidOperationException("No task provided.");

                    task.ContinueWith(delegate { syncContext.Complete(); }, TaskScheduler.Default);

                    // Pump continuations and propagate any exceptions.
                    syncContext.RunOnCurrentThread();
                    task.GetAwaiter().GetResult();
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(prevSyncContext);
                }
            });
        }

        /// <summary>
        /// Provides a SynchronizationContext that's single-threaded.
        /// </summary>
        private sealed class SingleThreadSynchronizationContext : SynchronizationContext, IDisposable
        {
            /// <summary>
            /// The queue of work items.
            /// </summary>
            private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> _queue = new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();
            
            /// <summary>
            /// Tracks whether Dispose has been called.
            /// </summary>
            private bool _isDisposed = false;

            /// <summary>
            /// Destroys an instance of SingleThreadSynchronizationContext.
            /// </summary>
            ~SingleThreadSynchronizationContext()
            {
                Dispose(false);
            }

            /// <summary>
            /// Dispatches an asynchronous message to the synchronization context.
            /// </summary>
            /// <param name="d">The System.Threading.SendOrPostCallback delegate to call.</param>
            /// <param name="state">The object passed to the delegate.</param>
            public override void Post(SendOrPostCallback d, object state)
            {
                if (d == null) throw new ArgumentNullException(nameof(d));

                // Possible race condition here when disposing of the BackendModule used.
                // This does the job but we should have a better understanding of the root cause.
                if (!_queue.IsAddingCompleted) {
                    _queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
                }
            }

            /// <summary>
            /// Dispatches a synchronous message to the synchronization context. Not supported.
            /// </summary>
            /// <param name="d">The System.Threading.SendOrPostCallback delegate to call.</param>
            /// <param name="state">The object passed to the delegate.</param>
            /// <exception cref="NotSupportedException"></exception>
            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("Synchronous sending is not supported.");
            }

            /// <summary>
            /// Runs an loop to process all queued work items.
            /// </summary>
            public void RunOnCurrentThread()
            {
                foreach (var workItem in _queue.GetConsumingEnumerable())
                {
                    workItem.Key(workItem.Value);
                }
            }

            /// <summary>
            /// Notifies the context that no more work will arrive.
            /// </summary>
            public void Complete()
            {
                _queue.CompleteAdding();
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
                if (_isDisposed) return;

                if (disposing) {
                    _queue.Dispose();
                }

                _isDisposed = true;
            }
        }
    }
}
