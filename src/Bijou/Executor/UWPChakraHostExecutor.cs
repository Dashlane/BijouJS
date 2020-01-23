using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Bijou.Async;
using Bijou.Chakra.Hosting;
using Bijou.JSTasks;
using Bijou.NativeFunctions;
using Bijou.Projected;

namespace Bijou.Executor
{
    /// <summary>
    /// Async JavaScript engine based on Chakra.
    /// </summary>
    /// <remarks>
    /// Do not use this class directly. Create an instance from UWPChakraExecutorFactory and access it though the IJsExecutorHost interface :
    ///     var executor = new UWPChakraExecutorFactory().CreateJsExecutorHost();
    ///     executor.LoadAndRunScriptAsync("path\\to\\my\\script.js");
    /// </remarks>
    public class UWPChakraHostExecutor
    {
        private bool _shouldStop;
        private bool _isDisposed;
        private JavaScriptRuntime _runtime;
        private readonly Task _jsTask;
        private GCHandle _thisHandle;

        // Native injected functions, required to keep a reference
        private readonly JavaScriptNativeFunction _setTimeoutJavaScriptNativeDelegate = JSAsyncFunctions.SetTimeoutJavaScriptNativeFunction;
        private readonly JavaScriptNativeFunction _setIntervalJavaScriptNativeDelegate = JSAsyncFunctions.SetIntervalJavaScriptNativeFunction;
        private readonly JavaScriptNativeFunction _clearScheduledFunctionJSNativeDelegate = JSAsyncFunctions.ClearScheduledJavaScriptNativeFunction;
        private readonly JavaScriptNativeFunction _sendToHostJavaScriptNativeFunction = JSSendToHost.SendToHostJavaScriptNativeFunction;
        private readonly JavaScriptPromiseContinuationCallback _promiseContinuationDelegate = JSAsyncFunctions.PromiseContinuationCallback;

        /// <summary>
        /// Task list handled by JS thread. Tasks are ordered by execution time. 
        /// </summary>
        private readonly List<AbstractJSTask> _sortedScheduledJSTaskList = new List<AbstractJSTask>();

        /// <summary>
        /// Native task dictionary handled by JS thread, used to keep reference of setTimeout/setInterval tasks.  
        /// </summary>
        private readonly Dictionary<int, AbstractJSTask> _cancellableJSTaskDictionary = new Dictionary<int, AbstractJSTask>();

        /// <summary>
        /// Pseudo-random generator, used to get the next JS task randomly.
        /// </summary>
        private readonly Random _random = new Random(Guid.NewGuid().GetHashCode());

        /// <summary>
        /// Native JS task queue handled by JS thread. Promise tasks have higher priority.
        /// </summary>
        private readonly Queue<JSTaskFunction> _jsPromiseQueue = new Queue<JSTaskFunction>();

        /// <summary>
        /// Thread-safe queue of JS tasks. Can be filled from other threads than the JS thread.
        /// </summary>
        private readonly AsyncQueue<AbstractJSTask> _jsTaskAsyncQueue = new AsyncQueue<AbstractJSTask>();

        /// <summary>
        /// Cancellation token source, used to cancel a wait & take operation on the blocking collection.
        /// </summary>
        private readonly CancellationTokenSource _jsCancellationSrc = new CancellationTokenSource();

        private readonly JavaScriptSourceContext _currentSourceContext = JavaScriptSourceContext.FromIntPtr(IntPtr.Zero);

        public event EventHandler<string> MessageReady;
        public event EventHandler<string> JsExecutionFailed;

        // Properties
        internal IntPtr InteropPointer => GCHandle.ToIntPtr(_thisHandle);

        /// <summary>
        /// Creates an instance of UWPChakraHostExecutor.
        /// Initializes runtime and context, injects native functions and starts the JS event loop.
        /// </summary>
        public UWPChakraHostExecutor()
        {
            try
            {
                // Needed for interoperability with C++.
                _thisHandle = GCHandle.Alloc(this);
            } 
            catch (ArgumentException e)
            {
                Debug.WriteLine("UWPChakraHostExecutor: GCHandle.Alloc raised exception: " + e.Message);
                throw;
            }

            // JS Runtime and Context are initialized in a background worker
            // thread, so that the JS Event Loop runs in the same thread.
            _jsTask = AsyncPump.Run(async delegate
            {
                InitializeJSExecutor();
#if DEBUG
                StartDebugging();
#endif
                await RunJSEventLoop();
            });
        }

        /// <summary>
        /// Destroys an instance of UWPChakraHostExecutor.
        /// </summary>
        ~UWPChakraHostExecutor()
        {
            Dispose(false);
        }

        /// <summary>
        /// Initializes JS runtime and context. Injects native functions.
        /// </summary>
        private void InitializeJSExecutor()
        {
            NativeMethods.ThrowIfError(NativeMethods.JsCreateRuntime(JavaScriptRuntimeAttributes.None, null, out _runtime));

            NativeMethods.ThrowIfError(NativeMethods.JsCreateContext(_runtime, out var context));

            NativeMethods.ThrowIfError(NativeMethods.JsSetCurrentContext(context));

            var globalObject = JavaScriptValue.GlobalObject;

            // ES6 Promise callback
            NativeMethods.ThrowIfError(NativeMethods.JsSetPromiseContinuationCallback(_promiseContinuationDelegate, InteropPointer));

            // Inject setTimeout and setInterval
            DefineHostCallback(globalObject, "setTimeout", _setTimeoutJavaScriptNativeDelegate, InteropPointer);
            DefineHostCallback(globalObject, "setInterval", _setIntervalJavaScriptNativeDelegate, InteropPointer);
            DefineHostCallback(globalObject, "clearTimeout", _clearScheduledFunctionJSNativeDelegate, InteropPointer);
            DefineHostCallback(globalObject, "clearInterval", _clearScheduledFunctionJSNativeDelegate, InteropPointer);
            DefineHostCallback(globalObject, "sendToHost", _sendToHostJavaScriptNativeFunction, InteropPointer);

            // Inject XmlHttpRequest projecting the namespace
            NativeMethods.ThrowIfError(NativeMethods.JsProjectWinRTNamespace("Frameworks.JsExecutor.UWP.Chakra.Native.Projected"));

            // Add references
            RunScript(@"const XMLHttpRequest = Frameworks.JsExecutor.UWP.Chakra.Native.Projected.XMLHttpRequest;
                        const console = Frameworks.JsExecutor.UWP.Chakra.Native.Projected.JSConsole;
                        const atob = Frameworks.JsExecutor.UWP.Chakra.Native.Projected.JSBase64Encoding.atob;
                        const btoa = Frameworks.JsExecutor.UWP.Chakra.Native.Projected.JSBase64Encoding.btoa;
                        console.log('UWPChakraHostExecutor ready');");

            // register to JSConsole 
            JSConsole.ConsoleMessageReady += OnConsoleMessageReady;
        }

        /// <summary>
        /// Starts the JS event loop.
        /// </summary>
        private async Task RunJSEventLoop()
        {
            // JS Event loop.
            // Check https://jakearchibald.com/2015/tasks-microtasks-queues-and-schedules/
            // for a detailed description of micro-task and task queues.
            while (!_shouldStop)
            {
                try
                {
                    // 1 - Consume promises queues (micro-task queue)
                    while (_jsPromiseQueue.Count > 0) 
                    {
                        var promise = _jsPromiseQueue.Dequeue();
                        promise.Execute();

                        // Usually, promises don't have an Id
                        // We might set it for debug purpose, so let's clean it up just in case
                        if (promise.HasValidId)
                        {
                            ClearCancellableTask(promise.Id);
                        }
                    }

                    // 2 - Execute scheduled tasks
                    var waitTimeout = -1;
                    while (_sortedScheduledJSTaskList.Count > 0)
                    {
                        var task = _sortedScheduledJSTaskList[0];
                        var timeToNextTask = task.MillisecondsToExecution;
                        if (timeToNextTask == 0) 
                        {
                            _sortedScheduledJSTaskList.RemoveAt(0);

                            // Execute the scheduled task
                            task.Execute();

                            // Reschedule if needed
                            if (task.ShouldReschedule)
                            {
                                task.ResetScheduledTime();
                                ScheduleTask(task);
                            } 
                            else if (task.HasValidId)
                            {
                                ClearCancellableTask(task.Id);
                            }
                        } 
                        else 
                        {
                            // Wait until next task execution time
                            waitTimeout = timeToNextTask;
                            break;
                        }
                    }

                    // 3 - Wait until next scheduled execution time for a task to be added from another thread
                    // Add new task to the scheduled list
                    // Need to wrap in try catch as cancel operation throws an OperationCanceledException
                    try
                    {
                        var jsTask = await _jsTaskAsyncQueue.DequeueAsync(waitTimeout);
                        if (jsTask == null)
                        {
                            continue;
                        }

                        if (jsTask.IsPromise)
                        {
                            AddPromise(jsTask as JSTaskFunction);
                        } 
                        else 
                        {
                            ScheduleTask(jsTask);
                        }
                    } 
                    catch (OperationCanceledException) 
                    {
                         Console.WriteLine("UWPChakraHostExecutor: TryTake operation was canceled");
                    }
                } 
                catch (JavaScriptScriptException scriptException) 
                {
                    var exceptionMessage = "UWPChakraHostExecutor: breaking js task loop, raised exception: " + scriptException.Message;
                    var errorMessage = scriptException.ErrorMessage;
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        exceptionMessage += ", JS message: " + errorMessage;
                    }

                    var errorFileName = scriptException.ErrorFileName;
                    if (!string.IsNullOrEmpty(errorFileName)) 
                    {
                        exceptionMessage += ", JS fileName: " + errorFileName;
                    }

                    var errorLine = scriptException.ErrorLineNumber;
                    if (!string.IsNullOrEmpty(errorLine)) 
                    {
                        exceptionMessage += ", JS line: " + errorLine;
                    }

                    Debug.WriteLine(exceptionMessage);
                    JsExecutionFailed?.Invoke(this, exceptionMessage);
                } 
                catch (JavaScriptUsageException usageException)
                {
                    var exceptionMessage = "UWPChakraHostExecutor: JavaScriptUsageException, " + usageException.Message;
                    Debug.WriteLine(exceptionMessage);
                    JsExecutionFailed?.Invoke(this, exceptionMessage);
                } 
                catch (Exception e)
                {
                    var exceptionMessage = "UWPChakraHostExecutor: breaking js task loop, raised exception: " + e.Message;
                    Console.Error.WriteLine(exceptionMessage);
                    JsExecutionFailed?.Invoke(this, exceptionMessage);
                    throw;
                }
            }

            // Un-attach the current JS context (otherwise we can't dispose of the runtime)
            NativeMethods.JsSetCurrentContext(JavaScriptContext.Invalid);
        }

        /// <summary>
        /// Loads a script from a file.
        /// </summary>
        public static string LoadScript(string filename)
        {
            if (!File.Exists(filename))
            {
                Debug.WriteLine("UWPChakraHostExecutor: unable to open file " + filename + ", file does not exist");
                return string.Empty;
            }

            var script = File.ReadAllText(filename);
            if (!string.IsNullOrEmpty(script))
            {
                return script;
            }

            Debug.WriteLine("UWPChakraHostExecutor: script file " + filename + " is empty");
            return string.Empty;
        }

        /// <summary>
        /// Loads a script and runs it by adding it to the event loop.
        /// </summary>
        public void LoadAndRunScriptAsync(string scriptPath)
        {
            var script = LoadScript(scriptPath);
            if (!string.IsNullOrEmpty(script)) 
            {
                RunScriptAsync(script, scriptPath);
            }
        }

        /// <summary>
        /// Runs a script by adding it to the event loop.
        /// </summary>
        public void RunScriptAsync(string script)
        {
            RunScriptAsync(script, string.Empty);
        }

        /// <summary>
        /// Runs a script by adding it to the event loop.
        /// </summary>
        public void RunScriptAsync(string script, string scriptPath)
        {
            AddTask(new JSTaskScript(scriptPath, script, _currentSourceContext));
        }

        private  void RunScript(string script)
        {
            RunScript(script, string.Empty);
        }

        private void RunScript(string script, string scriptPath)
        {
            if (!JavaScriptContext.IsCurrentValid) 
            {
                return;
            }

            var scriptTask = new JSTaskScript(scriptPath, script, _currentSourceContext);
            scriptTask.Execute();
        }

        public void CallFunctionAsync(string function, params object[] arguments)
        {
            var task = new JSTaskFunction(function, arguments);
            AddTask(task);
        }

        /// <summary>
        /// Adds a JS task to the event loop.
        /// </summary>
        internal void AddTask(AbstractJSTask task)
        {
            try 
            {
                if (!_shouldStop) 
                {
                    _jsTaskAsyncQueue.TryEnqueue(task);
                }
            } 
            catch (ObjectDisposedException) 
            {
                Debug.WriteLine("AddTask: _jsTaskCollection was disposed");
            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("AddTask: _jsTaskCollection was marked as complete");
            }
            catch (Exception e) 
            {
                Console.Error.WriteLine("AddTask: _jsTaskCollection.Add throw an exception, " + e.Message);
                throw;
            }
        }

        private void AddPromise(JSTaskFunction promise)
        {
            if (!JavaScriptContext.IsCurrentValid) 
            {
                return;
            }

            if (promise != null)
            {
                _jsPromiseQueue.Enqueue(promise);
            }
        }

        /// <summary>
        /// Adds a JS task that can be canceled.
        /// </summary>
        internal int AddCancellableTask(AbstractJSTask task)
        {
            if (!JavaScriptContext.IsCurrentValid)
            {
                return -1;
            }

            var taskId = GetNextCancellableTaskId();
            task.Id = taskId;
            _cancellableJSTaskDictionary.Add(taskId, task);
            AddTask(task);

            return taskId;
        }

        /// <summary>
        /// Cancels a JS task and prevents its execution in the event loop.
        /// </summary>
        internal void CancelTask(int taskId)
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

        /// <summary>
        /// Clears JS task from the task dictionary.
        /// </summary>
        private void ClearCancellableTask(int taskId)
        {
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

        /// <summary>
        /// Propagates the MessageReceived event to any registered subject.
        /// </summary>
        internal virtual void OnMessageReceived(string message)
        {
            MessageReady?.Invoke(this, message);
        }

        /// <summary>
        /// Schedules a task to be executed in the event loop.
        /// </summary>
        private void ScheduleTask(AbstractJSTask task)
        {
            if (!JavaScriptContext.IsCurrentValid) 
            {
                return;
            }

            var i = 0;
            while (i < _sortedScheduledJSTaskList.Count &&
                   _sortedScheduledJSTaskList[i].MillisecondsToExecution <= task.MillisecondsToExecution)
            {
                ++i;
            }

            _sortedScheduledJSTaskList.Insert(i, task);
        }

        /// <summary>
        /// Gets the next JS task.
        /// </summary>
        /// <returns></returns>
        private int GetNextCancellableTaskId()
        {
            // avoid collisions
            int nextId;
            do 
            {
                nextId = _random.Next();
            }
            while (_cancellableJSTaskDictionary.ContainsKey(_random.Next()));

            return nextId;
        }

        /// <summary>
        /// Injects a callback into JS.
        /// </summary>
        private static void DefineHostCallback(JavaScriptValue parentObject, string callbackName, JavaScriptNativeFunction callback, IntPtr callbackData)
        {
            var propertyId = JavaScriptPropertyId.FromString(callbackName);
            var function = JavaScriptValue.CreateFunction(callback, callbackData);

            parentObject.SetProperty(propertyId, function, true);
        }

        /// <summary>
        /// Starts debugging in the context.
        /// </summary>
        public static void StartDebugging()
        {
            if (!JavaScriptContext.IsCurrentValid)
            {
                return;
            }

            NativeMethods.ThrowIfError(NativeMethods.JsStartDebugging());
        }

        /// <summary>
        /// Writes to console.
        /// </summary>
        private void OnConsoleMessageReady(object sender, string logMessage)
        {
            Console.WriteLine(logMessage);
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
            Debug.WriteLine("Disposing UWPChakraHostExecutor");

            if (!_isDisposed)
            {
                _shouldStop = true;

                // mark thread-safe collection as complete with regards to additions
                _jsTaskAsyncQueue.Stop();

                // wait for the js thread to end
                _jsTask.Wait();

                if (_thisHandle.IsAllocated) 
                {
                    _thisHandle.Free();
                }

                if (disposing) {
                    _jsTask.Dispose();
                    _jsTaskAsyncQueue.Dispose();
                    _jsCancellationSrc.Dispose();
                    _runtime.Dispose();
                }

                _isDisposed = true;
            }

            Debug.WriteLine("UWPChakraHostExecutor disposed");
        }
    }
}
