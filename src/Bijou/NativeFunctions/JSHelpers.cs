using System;
using System.Runtime.InteropServices;

namespace Bijou.NativeFunctions
{
    internal static class JSHelpers
    {
        internal static BijouExecutor ExecutorFromCallbackData(IntPtr callbackData)
        {
            try 
            {
                var executorHandle = GCHandle.FromIntPtr(callbackData);
                return (executorHandle.Target as BijouExecutor);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}
