// UWPChakraHostExecutor.Main
/// <summary>
/// UWPChakraHostExecutor.Main creates an instance of JavaScript runtime engine
/// using Microsoft ChakraCore https://github.com/Microsoft/ChakraCore
/// How to use it, you ask? Thank you for asking
/// 1) Create an instance of UWPChakraHostExecutor. This
///    will:
///    a) create a background worker thread dedicated to JS engine
///    b) initialize a JS runtime 
///    c) initialize a JS context
///    d) set a callback for JS promises
///    e) set native functions that are not implemented in the E6 standard
///       (setTimeout, setInterval and console.log)
///    f) start listening for microtask (promises callbacks) and
///       task (setTimeout callbacks, scripts injected from the user)   
/// 2) Load and run a script by calling LoadAndRunScriptAsync(string scriptPath) or
///    run an hardcoded script by calling RunScriptAsync(string script)
/// </summary>

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
    /// Async JavaScript engine based on Chakra
    /// </summary>
    /// <remarks>
    /// Do not use this class directly. Create an instance from UWPChakraExecutorFactory and access it though the IJsExecutorHost interface :
    ///     var executor = new UWPChakraExecutorFactory().CreateJsExecutorHost();
    ///     executor.LoadAndRunScriptAsync("path\\to\\my\\script.js");
    /// </remarks>
    internal class UWPChakraHostExecutor : IJsExecutorHost
    {
        // Interop pointer handle
        private readonly GCHandle _thisHandle;

        // native injected functions, required to keep a reference
        private readonly JavaScriptNativeFunction _setTimeoutJavaScriptNativeDelegate = JSAsyncFunctions.SetTimeoutJavaScriptNativeFunction;
        private readonly JavaScriptNativeFunction _setIntervalJavaScriptNativeDelegate = JSAsyncFunctions.SetIntervalJavaScriptNativeFunction;
        private readonly JavaScriptNativeFunction _clearScheduledFunctionJSNativeDelegate = JSAsyncFunctions.ClearScheduledJavaScriptNativeFunction;
        private readonly JavaScriptNativeFunction _sendToHostJavaScriptNativeFunction = JSSendToHost.SendToHostJavaScriptNativeFunction;
        private readonly JavaScriptPromiseContinuationCallback _promiseContinuationDelegate = JSAsyncFunctions.PromiseContinuationCallback;

        // JavaScript thread
        private readonly Task _jsTask;

        // task list handled by JS thread, task are ordered by execution time
        private List<JSTaskAbstract> _jsSortedScheduledTasks = new List<JSTaskAbstract>();
        // native task dictionary handled by JS thread, used to keep reference of setTimeout/setInterval tasks
        private Dictionary<int, JSTaskAbstract> _cancellableJsTasksDictionary = new Dictionary<int, JSTaskAbstract>();
        private Random _random = new Random(Guid.NewGuid().GetHashCode());
        // native task queue handled by JS thread, promises tasks have higher priority
        private Queue<JSTaskFunction> _jsPromisesQueue = new Queue<JSTaskFunction>();

        // thread-safe collection, can be filled from other threads than JS
        private AsyncQueue<JSTaskAbstract> _jsTaskAsyncQueue = new AsyncQueue<JSTaskAbstract>();
        // cancellation token source, used to cancel a wait & take operation on the blocking collection
        private CancellationTokenSource _jsCancellationSrc = new CancellationTokenSource();
        private bool _shouldStop = false;

        // Track whether Dispose has been called
        bool _isDisposed = false;

        private JavaScriptSourceContext _currentSourceContext = JavaScriptSourceContext.FromIntPtr(IntPtr.Zero);
        private JavaScriptRuntime _runtime;

        // Events
        public event Action<string> MessageReady;
        public event Action<string> JsExecutionFailed;

        // logger
        private readonly ILogger _logger;

        // Properties
        internal IntPtr InteropPointer
        {
            get
            {
                return GCHandle.ToIntPtr(_thisHandle);
            }
        }

        /// <summary>
        ///     Creates an insance of UWPChakraHostExecutor
        ///     Initialize runtime and context
        ///     Inject native functions
        ///     Start the JS event loop
        /// </summary>
        public UWPChakraHostExecutor(ILogger logger = null)
        {
            // initialize logger
            _logger = logger;

            try {
                _thisHandle = GCHandle.Alloc(this);
            } catch (ArgumentException e) {
                Debug.WriteLine("UWPChakraHostExecutor: GCHAndle.Alloc raised exception: " + e.Message);
                throw;
            }
            // JS Runtime and Context are initialized in a background worker thread
            // So that the JS Event Loop runs n the same thread
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
        ///     Destroy an insance of UWPChakraHostExecutor
        /// </summary>
        ~UWPChakraHostExecutor()
        {
            // dispose the instance to clean-up 
            Dispose(false);
        }


        /// <summary>
        ///     Initialize JS runtime and context
        ///     Injetcs native functions
        /// </summary>
        private void InitializeJSExecutor()
        {
            JavaScriptContext context;

            NativeMethods.ThrowIfError(NativeMethods.JsCreateRuntime(JavaScriptRuntimeAttributes.None, null, out _runtime));

            NativeMethods.ThrowIfError(NativeMethods.JsCreateContext(_runtime, out context));

            NativeMethods.ThrowIfError(NativeMethods.JsSetCurrentContext(context));

            JavaScriptValue globalObject = JavaScriptValue.GlobalObject;

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
            JSConsole.ConsoleMessageReady += this.LogToFile;
        }

        /// <summary>
        ///     Start the JS event loop
        /// </summary>
        async private Task RunJSEventLoop()
        {
            // JS Event loop
            // check https://jakearchibald.com/2015/tasks-microtasks-queues-and-schedules/
            // for a detailed description of microtask and task queues
            while (!_shouldStop) {
                try {
                    // while(_jsPromisesCollection.count> 0 or _jsTaskCollection.count > 0 or _jsScheduledTaskCollection.count > 0)
                    // 1. _jsPromisesCollection.TryDequeue(out jsTask) ==> process and continue if exist
                    // 2. _jsScheduledTasks.Peek(out jsTask)
                    //             If timeout task > now ==> break (wait for signal ** with timeout for this task **)
                    //             If empty ==> break (wait without timeout)
                    //             Otherwise dequeue and exec
                    // 3. wait and insert all items from _jsTaskCollection to _jsScheduledTasks  (in timeout order)

                    // 1 - consume promises queues (microtask queue)
                    while (_jsPromisesQueue.Count > 0) {
                        var promise = _jsPromisesQueue.Dequeue();
                        promise.Execute();
                        // normally, promises don't have an Id
                        // we might set it for debug porpuse, so let's clean it up just in case
                        if (promise.HasValidId) {
                            ClearCancellableTask(promise.Id);
                        }
                    }

                    // 2 - execute scheduled tasks
                    int waitTimeout = -1;
                    while (_jsSortedScheduledTasks.Count > 0) {
                        var task = _jsSortedScheduledTasks[0];
                        int timeToNextTask = task.MillisecondsToExecution;
                        if (timeToNextTask == 0) {
                            _jsSortedScheduledTasks.RemoveAt(0);
                            // execute the scheduled task
                            task.Execute();
                            // reschedule if needed
                            if (task.ShouldReschedule) {
                                task.ResetScheduledTime();
                                ScheduleTask(task);
                            } else if (task.HasValidId) {
                                ClearCancellableTask(task.Id);
                            }
                        } else {
                            // wait until next task execution time
                            waitTimeout = timeToNextTask;
                            break;
                        }
                    }

                    JSTaskAbstract jsTask = null;
                    // 3 - Wait until next scheduled execution time for a task to be added from another thread
                    // Add new task to the scheduled list
                    // Need to wrap in try catch as cancel operation throws an OperationCanceledException
                    try {
                        jsTask = await _jsTaskAsyncQueue.DequeueAsync(waitTimeout);
                        if (jsTask != null) {
                            if (jsTask.IsPromise) {
                                AddPromise(jsTask as JSTaskFunction);
                            } else {
                                ScheduleTask(jsTask);
                            }
                        }
                    } catch (OperationCanceledException) {
                        _logger.Info("UWPChakraHostExecutor: TryTake operation was canceled");
                    }
                } catch (JavaScriptScriptException jsse) {
                    var exceptionMessage = "UWPChakraHostExecutor: breaking js task loop, raised exception: " + jsse.Message;
                    var errorMessage = jsse.ErrorMessage;
                    if (!String.IsNullOrEmpty(errorMessage)) {
                        exceptionMessage += ", JS message: " + errorMessage;
                    }
                    var errorFileName = jsse.ErrorFileName;
                    if (!String.IsNullOrEmpty(errorFileName)) {
                        exceptionMessage += ", JS fileName: " + errorFileName;
                    }
                    var errorLine = jsse.ErrorLineNumber;
                    if (!String.IsNullOrEmpty(errorLine)) {
                        exceptionMessage += ", JS line: " + errorLine;
                    }
                    _logger.Warn(exceptionMessage);
                    JsExecutionFailed?.Invoke(exceptionMessage);
                    continue;
                } catch (JavaScriptUsageException jsue) {
                    var exceptionMessage = "UWPChakraHostExecutor: JavaScriptUsageException, " + jsue.Message;
                    _logger.Warn(exceptionMessage);
                    JsExecutionFailed?.Invoke(exceptionMessage);
                    continue;
                } catch (Exception e) {
                    var exceptionMessage = "UWPChakraHostExecutor: breaking js task loop, raised exception: " + e.Message;
                    _logger.Error(exceptionMessage);
                    JsExecutionFailed?.Invoke(exceptionMessage);
                    throw;
                }
            }

            // Unattach the current JS context (otherwise we can't dispose of the runtime)
            NativeMethods.JsSetCurrentContext(JavaScriptContext.Invalid);
        }

        /// <summary>
        ///     Load a script from a file
        /// </summary>
        public static string LoadScript(string filename)
        {
            if (!File.Exists(filename)) {
                Debug.WriteLine("UWPChakraHostExecutor: unable to open file " + filename + ", file does not exist");
                return "";
            }

            string script = File.ReadAllText(filename);
            if (string.IsNullOrEmpty(script)) {
                Debug.WriteLine("UWPChakraHostExecutor: script file " + filename + " is empty");
                return "";
            }

            return script;
        }

        /// <summary>
        ///     Load a script and run it adding it to the event loop
        /// </summary>
        public void LoadAndRunScriptAsync(string scriptPath)
        {
            string script = LoadScript(scriptPath);

            if (!String.IsNullOrEmpty(script)) {
                RunScriptAsync(script, scriptPath);
            }
        }

        /// <summary>
        ///     Run a script adding it to the event loop
        /// </summary>
        public void RunScriptAsync(string script)
        {
            RunScriptAsync(script, "");
        }

        /// <summary>
        ///     Run a script adding it to the event loop
        ///     
        /// </summary>
        public void RunScriptAsync(string script, string scriptPath)
        {
            AddTask(new JSTaskScript(scriptPath, script, _currentSourceContext));
        }

        internal void RunScript(string script)
        {
            RunScript(script, "");
        }

        internal void RunScript(string script, string scriptPath)
        {
            if (!JavaScriptContext.IsCurrentValid) {
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
        ///     Add a js task to the event loop
        /// </summary>
        internal void AddTask(JSTaskAbstract task)
        {
            try {
                if (!_shouldStop) {
                    _jsTaskAsyncQueue.TryEnqueue(task);
                }
            } catch (ObjectDisposedException) {
                _logger.Info("AddTask: _jsTaskCollection was disposed");
            } catch (InvalidOperationException) {
                _logger.Info("AddTask: _jsTaskCollection was marked as complete");
            } catch (Exception e) {
                _logger.Error("AddTask: _jsTaskCollection.Add throwed an exception, " + e.Message);
                throw;
            }
        }

        private void AddPromise(JSTaskFunction promise)
        {
            if (!JavaScriptContext.IsCurrentValid) {
                return;
            }

            if (promise != null) {
                _jsPromisesQueue.Enqueue(promise);
            }
        }

        /// <summary>
        ///     Add a js task that can be canceled 
        /// </summary>
        internal virtual int AddCancellableTask(JSTaskAbstract task)
        {
            if (!JavaScriptContext.IsCurrentValid) {
                return -1;
            }

            int taskId = getNextCancellableTaskId();
            task.Id = taskId;
            _cancellableJsTasksDictionary.Add(taskId, task);
            AddTask(task);

            return taskId;
        }

        /// <summary>
        ///    Cancel a task and prevents its execution in the event loop
        /// </summary>
        internal virtual void CancelTask(int taskId)
        {
            if (!JavaScriptContext.IsCurrentValid) {
                return;
            }

            if (!_cancellableJsTasksDictionary.ContainsKey(taskId)) {
                _logger.Info("CancelTask: no task id found in task dictionary");
                return;
            }

            _cancellableJsTasksDictionary[taskId].Cancel();
        }

        /// <summary>
        ///     Clear task from the task dictionary
        /// </summary>
        private void ClearCancellableTask(int taskId)
        {
            if (!JavaScriptContext.IsCurrentValid) {
                return;
            }

            if (!_cancellableJsTasksDictionary.ContainsKey(taskId)) {
                _logger.Info("ClearCancellableTask: no task id found in task dictionary");
                return;
            }

            _cancellableJsTasksDictionary.Remove(taskId);
        }

        /// <summary>
        ///    Propagate message received event to any registered subject
        /// </summary>
        internal virtual void OnMessageReceived(string message)
        {
            MessageReady?.Invoke(message);
        }

        /// <summary>
        ///    Schedule a task to be executed in the event loop
        /// </summary>
        private void ScheduleTask(JSTaskAbstract task)
        {
            if (!JavaScriptContext.IsCurrentValid) {
                return;
            }

            int i = 0;
            while (i < _jsSortedScheduledTasks.Count && _jsSortedScheduledTasks[i].MillisecondsToExecution <= task.MillisecondsToExecution) {
                ++i;
                continue;
            }

            _jsSortedScheduledTasks.Insert(i, task);
        }

        private int getNextCancellableTaskId()
        {
            int nextId = -1;
            // avoid collisions
            do {
                nextId = _random.Next();
            }
            while (_cancellableJsTasksDictionary.ContainsKey(_random.Next()));

            return nextId;
        }


        /// <summary>
        ///     Inject a callback into JS
        /// </summary>
        private static void DefineHostCallback(JavaScriptValue parentObject, string callbackName, JavaScriptNativeFunction callback, IntPtr callbackData)
        {
            JavaScriptPropertyId propertyId = JavaScriptPropertyId.FromString(callbackName);
            JavaScriptValue function = JavaScriptValue.CreateFunction(callback, callbackData);

            parentObject.SetProperty(propertyId, function, true);
        }

        /// <summary>
        ///     Starts debugging in the context.
        /// </summary>
        public static void StartDebugging()
        {
            if (!JavaScriptContext.IsCurrentValid) {
                return;
            }
            NativeMethods.ThrowIfError(NativeMethods.JsStartDebugging());
        }

        /// <summary>
        ///     Log to filesystem
        /// </summary>
        private void LogToFile(object sender, string logMessage)
        {
            _logger.Info(logMessage);
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
            if (!_isDisposed) {
                _shouldStop = true;
                // mark thread-safe collection as complete with regards to additions
                _jsTaskAsyncQueue.Stop();
                // wait for the js thread to end
                _jsTask.Wait();

                if (_thisHandle.IsAllocated) {
                    _thisHandle.Free();
                }

                if (disposing) {
                    // dispose managed resources
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
