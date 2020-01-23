using System;
using System.Runtime.InteropServices;
using Bijou.Executor;

namespace Bijou.NativeFunctions
{
    internal static class JSHelpers
    {
        static internal UWPChakraHostExecutor ExecutorFromCallbackData(IntPtr callbackData)
        {
            try {
                GCHandle executorHandle = GCHandle.FromIntPtr(callbackData);
                return (executorHandle.Target as UWPChakraHostExecutor);
            } catch (InvalidOperationException) {
                return null;
            }
        }
    }
}
