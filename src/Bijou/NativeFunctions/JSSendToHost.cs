using System;
using Bijou.Chakra;

namespace Bijou.NativeFunctions
{
    internal static class JSSendToHost
    {
        /// <summary>
        /// JS Native function for sendToHost.
        /// JS function signature is sendToHost(message).
        /// message: string object
        /// </summary>
        /// <param name="callee"></param>
        /// <param name="isConstructCall"></param>
        /// <param name="arguments"></param>
        /// <param name="argumentCount"></param>
        /// <param name="callbackData"></param>
        /// <returns></returns>
        public static JavaScriptValue SendToHostJavaScriptNativeFunction(
            JavaScriptValue callee, 
            bool isConstructCall, 
            JavaScriptValue[] arguments, 
            ushort argumentCount, 
            IntPtr callbackData)
        {
            var executor = JSHelpers.ExecutorFromCallbackData(callbackData);
            if (executor == null) 
            {
                return JavaScriptValue.Invalid;
            }

            // Function signature is sendToHost(message:string).
            // Expected 2 arguments, as first argument is 'this'.
            if (arguments.Length != 2) 
            {
                return JavaScriptValue.Invalid;
            }

            // Skip arguments[0] as it is JavaScript 'this'.
            var jsMessage = arguments[1];
            if (!jsMessage.IsValid) 
            {
                return JavaScriptValue.Invalid;
            }

            if (jsMessage.ValueType.Value != JavaScriptValueType.String) 
            {
                return JavaScriptValue.Invalid;
            }

            executor.OnMessageReceived(jsMessage.AsString());

            return JavaScriptValue.Invalid;
        }
    }
}
