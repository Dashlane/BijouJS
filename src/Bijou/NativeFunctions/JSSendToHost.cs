using System;
using System.Diagnostics;
using Bijou.Chakra.Hosting;

namespace Bijou.NativeFunctions
{
    internal static class JSSendToHost
    {
        /// <summary>
        ///     JS Native function for sendToHost
        ///     JS function signature is sendToHost(message)
        ///     message: string object
        /// /// </summary>
        public static JavaScriptValue SendToHostJavaScriptNativeFunction(
            JavaScriptValue callee, 
            bool isConstructCall, 
            JavaScriptValue[] arguments, 
            ushort argumentCount, 
            IntPtr callbackData)
        {
            var ret = JavaScriptValue.Invalid;
            var executor = JSHelpers.ExecutorFromCallbackData(callbackData);
            if (executor == null) 
            {
                return ret;
            }

            // function signature is sendToHost(message:string)
            // expected 2 arguments, as first argument is this
            if (arguments.Length != 2) 
            {
                Debug.WriteLine("[SendToHostJavaScriptNativeFunction] Invalid arguments, received " + arguments.Length + " arguments, expected 2");
                return ret;
            }

            // arguments[0] is JavaScript "this"
            // we skip it
            var jsMessage = arguments[1];
            if (!jsMessage.IsValid) 
            {
                Debug.WriteLine("[SendToHostJavaScriptNativeFunction] Invalid argument");
                return ret;
            }

            if (jsMessage.ValueType != JavaScriptValueType.String) 
            {
                Debug.WriteLine("[SendToHostJavaScriptNativeFunction] Invalid argument type, received " + jsMessage.ValueType + ", expected String");
                return ret;
            }

            executor.OnMessageReceived(jsMessage.ToString());

            return ret;
        }
    }
}
