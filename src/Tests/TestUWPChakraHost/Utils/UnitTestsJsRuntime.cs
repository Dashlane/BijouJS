using System;
using Bijou.Chakra.Hosting;

namespace Bijou.Test.UWPChakraHost.Utils
{
    internal class UnitTestJsRuntime : IDisposable
    {
        private JavaScriptRuntime _runtime = JavaScriptRuntime.Create();
        
        internal UnitTestJsRuntime()
        {
            var context = _runtime.CreateContext();
            JavaScriptContext.Current = context;
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
                JavaScriptContext.Current = JavaScriptContext.Invalid;
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