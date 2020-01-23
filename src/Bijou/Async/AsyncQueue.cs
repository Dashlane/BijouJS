using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bijou.Async
{
    /// <summary>A thread-safe, asynchronously dequeuable queue.</summary> 
    public class AsyncQueue<T> : IDisposable
    {
        /// <summary>Lightweight semaphore, that can be asynchronously waited</summary> 
        private readonly SemaphoreSlim _semaphore;
        /// <summary>Thread-safe first in-first out (FIFO) collection</summary> 
        private readonly ConcurrentQueue<T> _queue;
        /// <summary>Cancellation token source, used to stop the async queue and prevent </summary> 
        CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        /// <summary>Track whether Dispose has been called.</summary>
        bool _isDisposed = false;

        /// <summary>Gets whether the AsyncQueue has been stopped </summary> 
        public bool IsStopped => _cancellationTokenSource.IsCancellationRequested;

        /// <summary>Creates a new instance of AsyncQueue</summary> 
        public AsyncQueue()
        {
            _semaphore = new SemaphoreSlim(0);
            _queue = new ConcurrentQueue<T>();
        }

        /// <summary>Destructs the instance of AsyncQueue</summary>
        ~AsyncQueue()
        {
            Dispose(false);
        }

        /// <summary>Adds an element to the tail of the queue if it has not yet completed.</summary> 
        public bool TryEnqueue(T item)
        {
            return TryEnqueueRange(Enumerable.Empty<T>().Append(item));
        }

        /// <summary>Adds a range of element to the tail of the queue if it has not yet completed.</summary> 
        public bool TryEnqueueRange(IEnumerable<T> source)
        {
            if (_cancellationTokenSource.IsCancellationRequested) {
                return false;
            }
            // validate argument
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }
            var n = 0;
            foreach (var item in source) {
                _queue.Enqueue(item);
                n++;
            }
            _semaphore.Release(n);
            return true;
        }

        /// <summary>Gets a task whose result is the element at the head of the queue.</summary> 
        public async Task<T> DequeueAsync(int waitTimeout = -1)
        {
            for ( ; ; )
            {
                try {
                    await _semaphore.WaitAsync(waitTimeout, _cancellationTokenSource.Token);
                } catch (OperationCanceledException) {
                    // async queue has been stopped
                    // try to dequeue last item
                    Debug.WriteLine("AsyncQueue.DequeueAsync: queue stopped");
                } catch (ObjectDisposedException) {
                    // object disposed
                    // try to dequeue last item
                    Debug.WriteLine("AsyncQueue.DequeueAsync: queue disposed");
                } catch (ArgumentOutOfRangeException) {
                    Debug.WriteLine("AsyncQueue.DequeueAsync: ArgumentOutOfRangeException exception");
                    throw;
                }

                _queue.TryDequeue(out T item);
                return item;
            }
        }

        /// <summary>Stops the queue, canceling any DequeueAsync request</summary> 
        public void Stop()
        {
            _cancellationTokenSource.Cancel();
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
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed) {
                if (disposing) {
                    _cancellationTokenSource.Dispose();
                    _semaphore.Dispose();
                }

                _isDisposed = true;
            }
        }
    }
}
