using System;
using Bijou.Chakra;
using Bijou.Types;

namespace Bijou.NativeFunctions
{
    internal static class JSSendToHost
    {
        /// <summary>
        /// JS Native function for sendToHost.
        /// JS function signature is sendToHost(message).
        /// message: string object
        /// /// </summary>
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

            // function signature is sendToHost(message:string)
            // expected 2 arguments, as first argument is this
            if (arguments.Length != 2) 
            {
                return JavaScriptValue.Invalid;
            }

            // arguments[0] is JavaScript "this"
            // we skip it
            var jsMessage = arguments[1].ToObject();
            if (!jsMessage.IsValid) 
            {
                return JavaScriptValue.Invalid;
            }

            if (!(jsMessage is JavaScriptString text)) 
            {
                return JavaScriptValue.Invalid;
            }

            executor.OnMessageReceived(text.AsString().Value);

            return JavaScriptValue.Invalid;
        }
    }
}
