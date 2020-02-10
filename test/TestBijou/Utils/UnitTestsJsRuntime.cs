using Bijou.Chakra;
using FluentResults;
using System;

namespace Bijou.Test.UWPChakraHost.Utils
{
    internal class UnitTestJsRuntime : IDisposable
    {
        private JavaScriptRuntime _runtime;
        
        internal UnitTestJsRuntime()
        {
            var runtime = NativeMethods.JsCreateRuntime(JavaScriptRuntimeAttributes.None, null);
            if (runtime.IsFailed)
            {
                return;
            }

            _runtime = runtime.Value;

            var context = NativeMethods.JsCreateContext(_runtime);
            if (context.IsFailed)
            {
                return;
            }

            var contextSetup = NativeMethods.JsSetCurrentContext(context.Value);
            if (contextSetup.IsFailed)
            {
                return;
            }

            var globalObject = JavaScriptValue.GlobalObject;
            if (globalObject.IsFailed)
            {
                return;
            }
        }

        #region IDisposable

        private bool _isDisposed;

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                JavaScriptContext.Current = Results.Ok(JavaScriptContext.Invalid);
                _runtime.Dispose();
            }

            _isDisposed = true;
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion
    }
}