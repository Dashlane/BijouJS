using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bijou.Async;
using Bijou.Chakra;
using Bijou.JSTasks;
using FluentResults;

namespace Bijou
{
    /// <summary>
    /// This class represents the core of the engine: the event loop that executes promises and tasks.
    /// </summary>
    internal class EventLoop : IDisposable
    {
        /// <summary>
        /// State of the loop.
        /// </summary>
        private enum EventLoopStatus
        {
            Idle,
            Running,
            Terminated
        }

        /// <summary>
        /// Current state of the loop.
        /// </summary>
        private volatile EventLoopStatus _status = EventLoopStatus.Idle;

        /// <summary>
        /// Thread-safe collection, can be filled from other threads than JS.
        /// </summary>
        private readonly AsyncQueue<EventLoopTask> _pendingTasks = new AsyncQueue<EventLoopTask>();

        /// <summary>
        /// Task list handled by JS thread, task are ordered by execution time.
        /// </summary>
        private readonly SortedList<int, EventLoopTask> _scheduledTasks = new SortedList<int, EventLoopTask>();

        /// <summary>
        /// Native task queue handled by JS thread. Promises tasks have higher priority.
        /// </summary>
        private readonly Queue<EventLoopTask> _promises = new Queue<EventLoopTask>();

        /// <summary>
        /// Native task dictionary handled by JS thread, used to keep reference of setTimeout/setInterval tasks.  
        /// </summary>
        private readonly Dictionary<int, AbstractJSTask> _cancellableJSTaskDictionary = new Dictionary<int, AbstractJSTask>();

        #region Events

        /// <summary>
        /// Event raised when the event loop had to stop because an exception was raised.
        /// </summary>
        public event EventHandler<InvalidOperationException> ExecutionFailed;

        #endregion

        #region Public API

        /// <summary>
        /// Starts the loop.
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            DisposeGuard();

            if (_status == EventLoopStatus.Running)
            {
                return;
            }

            _status = EventLoopStatus.Running;

            // JS Event loop
            // check https://jakearchibald.com/2015/tasks-microtasks-queues-and-schedules/
            // for a detailed description of micro-task and task queues
            while (_status == EventLoopStatus.Running)
            {
                try
                {
                    // 1 - Consume promises queues (micro-task queue)
                    ExecutePromises();

                    // 2 - Execute scheduled tasks
                    var waitTimeout = ExecuteScheduledTasks();

                    // 3 - Wait until next scheduled execution time for a task to be added from another thread
                    // Add new task to the scheduled list
                    // Need to wrap in try catch as cancel operation throws an OperationCanceledException
                    await WaitForTasks(waitTimeout);
                }
                catch (Exception e)
                {
                    var exceptionMessage = "UWPChakraHostExecutor: breaking js task loop, raised exception: " + e.Message;
                    Console.Error.WriteLine(exceptionMessage);
                    ExecutionFailed?.Invoke(this, new InvalidOperationException(exceptionMessage));
                    throw;
                }
            }

            // Un-attach the current JS context (otherwise we can't dispose of the runtime)
            NativeMethods.JsSetCurrentContext(JavaScriptContext.Invalid);
        }

        /// <summary>
        /// Stops the loop.
        /// </summary>
        public void Stop()
        {
            DisposeGuard();

            _status = EventLoopStatus.Terminated;
            _pendingTasks.Stop();
        }

        /// <summary>
        /// Adds a JS task that returns a value to the event loop.
        /// </summary>
        /// <typeparam name="TValue">Type of returned value</typeparam>
        /// <param name="task">JS Task to add to the loop</param>
        /// <returns></returns>
        public Task<Result> Push(AbstractJSTask task)
        {
            try
            {
                var completion = new TaskCompletionSource<Result>();
                _pendingTasks.TryEnqueue(new EventLoopTask(task, completion.SetResult));

                return completion.Task;
            }
            catch (ObjectDisposedException)
            {
                Debug.WriteLine("AddTask: _jsTaskCollection was disposed");
                throw;
            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("AddTask: _jsTaskCollection was marked as complete");
                throw;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("AddTask: _jsTaskCollection.Add throw an exception, " + e.Message);
                throw;
            }
        }

        /// <summary>
        /// Adds a JS task to the event loop.
        /// </summary>
        /// <param name="task">JS Task to add to the loop</param>
        /// <returns>Id of the task</returns>
        public int PushTask(AbstractJSTask task)
        {
            try
            {
                _pendingTasks.TryEnqueue(new EventLoopTask(task, c => { }));

                return task.Id.GetHashCode();
            }
            catch (ObjectDisposedException)
            {
                Debug.WriteLine("AddTask: _jsTaskCollection was disposed");
                throw;
            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("AddTask: _jsTaskCollection was marked as complete");
                throw;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("AddTask: _jsTaskCollection.Add throw an exception, " + e.Message);
                throw;
            }
        }

        /// <summary>
        /// Cancels a JS task and prevents its execution in the event loop.
        /// </summary>
        public virtual void CancelTask(int taskId)
        {
            if (!JavaScriptContext.IsCurrentValid)
            {
                return;
            }

            if (!_cancellableJSTaskDictionary.ContainsKey(taskId))
            {
                Debug.WriteLine($"CancelTask: no task of id {taskId} found in task dictionary");
                return;
            }

            _cancellableJSTaskDictionary[taskId].Cancel();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Executes all the promises in the queue. First step of the loop.
        /// </summary>
        private void ExecutePromises()
        {
            while (_promises.Count > 0)
            {
                var task = _promises.Dequeue();
                var promise = task.Task;
                var result = promise.Execute();
                task.CompleteTask(result);

                // Usually, promises don't have an Id
                // We might set it for debug purpose, so let's clean it up just in case
                ClearCancellableTask(promise);
            }
        }

        /// <summary>
        /// Executes the tasks added with setTimeout().
        /// </summary>
        /// <returns></returns>
        private int ExecuteScheduledTasks()
        {
            var waitTimeout = -1;
            while (_scheduledTasks.Count > 0)
            {
                var task = _scheduledTasks.First().Value;
                var timeToNextTask = task.Task.MillisecondsToExecution;
                if (timeToNextTask == 0)
                {
                    _scheduledTasks.RemoveAt(0);

                    // Execute the scheduled task.
                    var result = task.Task.Execute();
                    task.CompleteTask(result);

                    // Reschedule if needed.
                    if (task.Task.ShouldReschedule)
                    {
                        task.Task.ResetScheduledTime();
                        AddScheduledTask(task);
                    }
                    else
                    {
                        ClearCancellableTask(task.Task);
                    }
                }
                else
                {
                    // Wait until next task execution time.
                    waitTimeout = timeToNextTask;
                    break;
                }
            }

            return waitTimeout;
        }

        /// <summary>
        /// Wait until next scheduled execution time for a task to be added from another thread.
        /// </summary>
        /// <param name="waitTimeout"></param>
        /// <returns></returns>
        private async Task WaitForTasks(int waitTimeout)
        {
            try
            {
                var jsTask = await _pendingTasks.DequeueAsync(waitTimeout);
                var task = jsTask.Task;
                if (task == null)
                {
                    return;
                }

                if (task.IsPromise)
                {
                    AddPromise(jsTask);
                }
                else
                {
                    AddScheduledTask(jsTask);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("UWPChakraHostExecutor: TryTake operation was canceled");
            }
        }

        /// <summary>
        /// Adds a promise to the promise queue.
        /// </summary>
        /// <param name="promise"></param>
        private void AddPromise(EventLoopTask promise)
        {
            if (!JavaScriptContext.IsCurrentValid)
            {
                return;
            }

            _promises.Enqueue(promise);
        }

        /// <summary>
        /// Schedules a task to be executed in the event loop.
        /// </summary>
        private void AddScheduledTask(EventLoopTask task)
        {
            if (!JavaScriptContext.IsCurrentValid)
            {
                return;
            }

            _scheduledTasks.Add(task.Task.MillisecondsToExecution, task);
        }

        /// <summary>
        /// Adds a JS task that can be canceled to the event loop.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        internal virtual int AddCancellableTask(AbstractJSTask task)
        {
            if (!JavaScriptContext.IsCurrentValid)
            {
                return -1;
            }

            var key = task.Id.GetHashCode();
            _cancellableJSTaskDictionary.Add(key, task);
            _pendingTasks.TryEnqueue(new EventLoopTask(task));

            return key;
        }

        /// <summary>
        /// Clears JS task from the task dictionary.
        /// </summary>
        internal virtual void ClearCancellableTask(AbstractJSTask task)
        {
            var taskId = task.Id.GetHashCode();
            if (!JavaScriptContext.IsCurrentValid)
            {
                return;
            }

            if (!_cancellableJSTaskDictionary.ContainsKey(taskId))
            {
                Debug.WriteLine("ClearCancellableTask: no task id found in task dictionary");
                return;
            }

            _cancellableJSTaskDictionary.Remove(taskId);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_status == EventLoopStatus.Terminated)
            {
                return;
            }

            _status = EventLoopStatus.Terminated;

            _pendingTasks.Dispose();
        }

        private void DisposeGuard()
        {
            if (_status != EventLoopStatus.Terminated)
            {
                return;
            }

            throw new ObjectDisposedException(nameof(EventLoop));
        }
        #endregion
    }
}