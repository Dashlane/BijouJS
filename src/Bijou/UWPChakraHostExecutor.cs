using Bijou.Async;
using Bijou.JSTasks;
using Bijou.NativeFunctions;
using Bijou.Projected;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Bijou.Chakra;

namespace Bijou
{
    /// <summary>
    /// Async JavaScript engine based on Chakra.
    /// </summary>
    public class UWPChakraHostExecutor : IDisposable
    {
        private bool _isDisposed;
        private JavaScriptRuntime _runtime;
        private GCHandle _thisHandle;
        private readonly Task _jsTask;
        private readonly EventLoop _eventLoop = new EventLoop();

        /// <summary>
        /// Native injected functions, required to keep a reference.
        /// </summary>
        private readonly List<JavaScriptNativeFunction> _nativeFunctions = new List<JavaScriptNativeFunction>();

        /// <summary>
        /// Pseudo-random generator, used to get the next JS task randomly.
        /// </summary>
        private readonly Random _random = new Random(Guid.NewGuid().GetHashCode());

        /// <summary>
        /// Cancellation token source, used to cancel a wait & take operation on the blocking collection
        /// </summary>
        private readonly CancellationTokenSource _jsCancellationSrc = new CancellationTokenSource();

        private readonly JavaScriptSourceContext _currentSourceContext = JavaScriptSourceContext.FromIntPtr(IntPtr.Zero);

        #region Events

        public event EventHandler<string> MessageReady;
        public event EventHandler<string> JsExecutionFailed;

        #endregion

        #region Properties

        internal IntPtr InteropPointer => GCHandle.ToIntPtr(_thisHandle);

        #endregion

        #region Initialization

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
                await _eventLoop.Run();
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
        private Result InitializeJSExecutor()
        {
            var runtime = NativeMethods.JsCreateRuntime(JavaScriptRuntimeAttributes.None, null);
            if (runtime.IsFailed)
            {
                return Results.Fail("Failed to initialize runtime");
            }

            _runtime = runtime.Value;

            var context = NativeMethods.JsCreateContext(_runtime);
            if (context.IsFailed)
            {
                return Results.Fail("Failed to initialize context");
            }

            var contextSetup = NativeMethods.JsSetCurrentContext(context.Value);
            if (contextSetup.IsFailed)
            {
                return Results.Fail("Failed to set the context");
            }

            // ES6 Promise callback
            NativeMethods.JsSetPromiseContinuationCallback(JSAsyncFunctions.PromiseContinuationCallback(_eventLoop.PushTask), InteropPointer);

            var globalObject = JavaScriptValue.GlobalObject;
            if (globalObject.IsFailed)
            {
                return Results.Fail("Failed to retrieve global object");
            }

            // Inject setTimeout and setInterval
            DefineHostCallback(globalObject.Value, "setTimeout", JSAsyncFunctions.SetTimeoutJavaScriptNativeFunction(_eventLoop.PushTask), InteropPointer);
            DefineHostCallback(globalObject.Value, "setInterval", JSAsyncFunctions.SetIntervalJavaScriptNativeFunction(_eventLoop.PushTask), InteropPointer);
            DefineHostCallback(globalObject.Value, "clearTimeout", JSAsyncFunctions.ClearScheduledJavaScriptNativeFunction(_eventLoop.CancelTask), InteropPointer);
            DefineHostCallback(globalObject.Value, "clearInterval", JSAsyncFunctions.ClearScheduledJavaScriptNativeFunction(_eventLoop.CancelTask), InteropPointer);
            DefineHostCallback(globalObject.Value, "sendToHost", JSSendToHost.SendToHostJavaScriptNativeFunction, InteropPointer);

            // Inject XmlHttpRequest projecting the namespace
            NativeMethods.JsProjectWinRTNamespace("Frameworks.JsExecutor.UWP.Chakra.Native.Projected");

            // Add references
            RunScript(@"const XMLHttpRequest = Bijou.Projected.XMLHttpRequest;
                        const console = Bijou.Projected.JSConsole;
                        const atob = Bijou.Projected.JSBase64Encoding.atob;
                        const btoa = Bijou.Projected.JSBase64Encoding.btoa;
                        console.log('UWPChakraHostExecutor ready');");

            // register to JSConsole 
            JSConsole.ConsoleMessageReady += OnConsoleMessageReady;
            
            return Results.Ok();
        }

        #endregion

        #region IDisposable

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
                _isDisposed = true;

                // wait for the js thread to end
                _eventLoop.Stop();
                _jsTask.Wait();

                if (_thisHandle.IsAllocated)
                {
                    _thisHandle.Free();
                }

                if (disposing)
                {
                    _jsTask.Dispose();
                    _eventLoop.Dispose();
                    _jsCancellationSrc.Dispose();
                    _runtime.Dispose();
                }
            }

            Debug.WriteLine("UWPChakraHostExecutor disposed");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Starts debugging in the context.
        /// </summary>
        public static Result StartDebugging()
        {
            return !JavaScriptContext.IsCurrentValid ?
                Results.Fail("Failed to start debugging") :
                NativeMethods.JsStartDebugging();
        }

        /// <summary>
        /// Load a script from a file.
        /// </summary>
        public static async Task<string> LoadScriptAsync(Uri scriptUri)
        {
            if (scriptUri == null)
            {
                return string.Empty;
            }

            StorageFile file;
            try
            {
                file = await StorageFile.GetFileFromApplicationUriAsync(scriptUri);
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine($"{nameof(UWPChakraHostExecutor)}: unable to open file {scriptUri}, file does not exist");
                return string.Empty;
            }

            if (file == null || !file.IsAvailable)
            {
                Debug.WriteLine($"{nameof(UWPChakraHostExecutor)}: unable to open file {scriptUri}, file is not available");
                return string.Empty;
            }

            var script = await FileIO.ReadTextAsync(file);

            if (string.IsNullOrEmpty(script))
            {
                Debug.WriteLine($"{nameof(UWPChakraHostExecutor)}: file {scriptUri} is empty");
            }

            return script;
        }

        /// <summary>
        /// Load a script and run it adding it to the event loop.
        /// </summary>
        public async Task<Result> RunScriptAsync(Uri scriptUri)
        {
            var script = await LoadScriptAsync(scriptUri);

            return !string.IsNullOrEmpty(script) ? await RunScriptAsync(script, scriptUri.AbsolutePath)
                                                 : await new Task<Result>(Results.Ok);
        }

        /// <summary>
        /// Run a script by adding it to the event loop.
        /// </summary>
        public Task<Result> RunScriptAsync(string script)
        {
            return RunScriptAsync(script, string.Empty);
        }

        /// <summary>
        /// Run a script by adding it to the event loop.
        /// </summary>
        public Task<Result> RunScriptAsync(string script, string scriptPath)
        {
            CheckDisposed();

            return _eventLoop.Push(new JSTaskScript(scriptPath, script, _currentSourceContext));
        }

        /// <summary>
        /// Execute a function by adding it to the event loop.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="function"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public Task<Result> CallFunctionAsync(string function, params object[] arguments)
        {
            CheckDisposed();

            return _eventLoop.Push(new JSTaskFunction(function, arguments));
        }

        #endregion

        #region Guard

        private void CheckDisposed()
        {
            if (!_isDisposed)
            {
                return;
            }

            throw new ObjectDisposedException(nameof(UWPChakraHostExecutor));
        }

        #endregion

        /// <summary>
        /// Runs the script immediately and synchronously.
        /// </summary>
        /// <param name="script"></param>
        /// <param name="scriptPath"></param>
        internal void RunScript(string script, string scriptPath = "")
        {
            if (!JavaScriptContext.IsCurrentValid)
            {
                return;
            }

            var scriptTask = new JSTaskScript(scriptPath, script, _currentSourceContext);
            scriptTask.Execute();
        }

        /// <summary>
        /// Propagates the MessageReceived event to any registered subject.
        /// </summary>
        internal virtual void OnMessageReceived(string message)
        {
            MessageReady?.Invoke(this, message);
        }

        /// <summary>
        /// Inject a callback into JS
        /// </summary>
        private void DefineHostCallback(JavaScriptValue parentObject, string callbackName, JavaScriptNativeFunction callback, IntPtr callbackData)
        {
            var function = NativeMethods.JsCreateFunction(callback, callbackData);

            parentObject.SetProperty(callbackName, function.Value, true);
            _nativeFunctions.Add(callback);
        }

        /// <summary>
        /// Log to filesystem
        /// </summary>
        private void OnConsoleMessageReady(object sender, string logMessage)
        {
            Console.WriteLine(logMessage);
        }
    }
}
