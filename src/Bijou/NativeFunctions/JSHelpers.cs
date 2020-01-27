using System;
using System.Runtime.InteropServices;

namespace Bijou.NativeFunctions
{
    internal static class JSHelpers
    {
        internal static UWPChakraHostExecutor ExecutorFromCallbackData(IntPtr callbackData)
        {
            try 
            {
                var executorHandle = GCHandle.FromIntPtr(callbackData);
                return (executorHandle.Target as UWPChakraHostExecutor);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}
