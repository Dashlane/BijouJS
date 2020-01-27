using System;
using System.Collections.Generic;
using Bijou.Chakra;
using Bijou.JSTasks;
using Bijou.Types;
using FluentResults;

namespace Bijou.NativeFunctions
{
    internal static class JSAsyncFunctions
    {
        /// <summary>
        /// JS Native function for setTimeout
        /// </summary>
        /// <remarks>
        /// Syntax of
        ///     setTimeout(function, milliseconds, param1, param2, ...)
        /// Parameter Values
        ///     function:            Required. The function to be executed
        ///     milliseconds:        Optional. The number of milliseconds to wait before executing the code. If omitted, the value 0 is used
        ///     param1, param2, ...:	Optional. Additional parameters to pass to the function
        /// Return Value
        ///     A Number, representing the ID value of the timer that is set.Use this value with the clearInterval() method to cancel the timer
        /// </remarks>
        public static JavaScriptNativeFunction SetTimeoutJavaScriptNativeFunction(PushTask pushHandler)
        {
            return (callee, isConstructCall, arguments, argumentCount, callbackData) =>
            {
                if (argumentCount >= 2 && arguments.Length >= 2)
                {
                    var result = AddScheduledJavaScriptNativeFunction(pushHandler, arguments, callbackData, 0).Value;

                    return result.UnderlyingValue;
                }

                Console.Error.WriteLine(
                    "[SetTimeoutJavaScriptNativeFunction] Invalid argumentCount, expected >= 2, received " +
                    argumentCount);
                return JavaScriptValue.Invalid;
            };
        }

        /// <summary>
        /// JS Native function for setInterval
        /// </summary>
        /// <remarks>
        /// Syntax of
        ///     setInterval(function, milliseconds, param1, param2, ...)
        /// Parameter Values
        ///     function:            Required. The function to be executed
        ///     milliseconds:        Required. The intervals (in milliseconds) on how often to execute the code. If the value is less than 10, the value 10 is used
        ///     param1, param2, ...:	Optional. Additional parameters to pass to the function
        /// Return Value
        ///     A Number, representing the ID value of the timer that is set.Use this value with the clearInterval() method to cancel the timer
        /// </remarks>
        public static JavaScriptNativeFunction SetIntervalJavaScriptNativeFunction(PushTask pushHandler)
        {
            return (callee, isConstructCall, arguments, argumentCount, callbackData) =>
            {
                var ret = JavaScriptValue.Invalid;
                if (argumentCount < 3 || arguments.Length < 3)
                {
                    Console.Error.WriteLine(
                        "[SetIntervalJavaScriptNativeFunction] Invalid argumentCount, expected >= 3, received " +
                        argumentCount);
                    return ret;
                }

                return AddScheduledJavaScriptNativeFunction(pushHandler, arguments, callbackData, 10, true).Value.UnderlyingValue;
            };
        }

        /// <summary>
        /// JS Native function for clear async operation (clearTimeout / clearInterval)
        /// </summary>
        /// <remarks>
        /// Syntax of
        ///     clearTimeout/clearInterval(id)
        /// Parameter Values
        ///     var:  Required. The ID of the timer returned by the setTimeout/setInterval() method
        /// Return Value
        ///     No return value
        /// </remarks>
        public static JavaScriptNativeFunction ClearScheduledJavaScriptNativeFunction(CancelTask cancelHandler)
        {
            return (callee, isConstructCall, arguments, argumentCount, callbackData) =>
            {
                var ret = JavaScriptValue.Invalid;
                if (argumentCount != 2 || arguments.Length != 2)
                {
                    Console.Error.WriteLine(
                        "[ClearScheduledJavaScriptNativeFunction] Invalid argumentCount, expected  2, received " +
                        arguments.Length);
                    return ret;
                }

                var executor = JSHelpers.ExecutorFromCallbackData(callbackData);
                if (executor == null)
                {
                    Console.Error.WriteLine("[ClearScheduledJavaScriptNativeFunction] Invalid executor");
                    return ret;
                }

                // clearTimeout / clearInterval signature is (id)
                var arg = arguments[1].ToObject();
                if (!arg.IsValid)
                {
                    Console.Error.WriteLine("[ClearScheduledJavaScriptNativeFunction] Invalid argument");
                    return ret;
                }

                if (!(arg is JavaScriptNumber id))
                {
                    Console.Error.WriteLine(
                        "[ClearScheduledJavaScriptNativeFunction] Invalid argument type, expected Number, received " +
                        arg.GetType());
                    return ret;
                }

                var taskId = id.AsInt32();
                cancelHandler(taskId.Value);

                return ret;
            };
        }

        /// <summary>
        /// JS Native function to use as a callback for ES6 promises.
        /// </summary>
        public static JavaScriptPromiseContinuationCallback PromiseContinuationCallback(PushTask pushHandler)
        {
            return (promise, callbackData) =>
            {
                var executor = JSHelpers.ExecutorFromCallbackData(callbackData);
                if (executor == null)
                {
                    Console.Error.WriteLine("[PromiseContinuationCallback] Error parsing callbackData");
                    return;
                }

                if (!JavaScriptContext.IsCurrentValid)
                {
                    Console.Error.WriteLine("[PromiseContinuationCallback] Invalid Context");
                    return;
                }

                var globalObject = JavaScriptPrototype.GlobalObject;
                var task = new JSTaskFunction(new JavaScriptFunction(promise),
                    new JavaScriptObject[] {globalObject.Value}) {IsPromise = true};
                pushHandler(task);
            };
        }

        /// <summary>
        /// JS Native function for generic async operation (setTimeout / setInterval)
        /// </summary>
        private static Result<JavaScriptNumber> AddScheduledJavaScriptNativeFunction(
            PushTask pushHandler,
            IReadOnlyList<JavaScriptValue> arguments,
            IntPtr callbackData,
            int minDelay,
            bool repeat = false)
        {
            try
            {
                var executor = JSHelpers.ExecutorFromCallbackData(callbackData);
                if (executor == null)
                {
                    return Results.Fail("[AddScheduledJavaScriptNativeFunction] Invalid executor");
                }

                // check arguments
                if (arguments.Count < 2)
                {
                    return Results.Fail("[AddScheduledJavaScriptNativeFunction] Invalid argumentCount, expected >= 2, received " + arguments.Count);
                }

                // arguments[0] is JavaScript this
                var callerObject = arguments[0].ToObject();

                // setTimeout / setInterval signature is (callback, [after, params...])
                var callback = new JavaScriptFunction(arguments[1]);

                var delay = minDelay;
                if (arguments.Count > 2)
                {
                    var afterValue = arguments[2].ToObject();
                    if (!afterValue.IsValid)
                    {
                        return Results.Fail("[AddScheduledJavaScriptNativeFunction] Invalid delay argument, expected Number, received Invalid");
                    }

                    if (!(afterValue is JavaScriptNumber val))
                    {
                        return Results.Fail("[AddScheduledJavaScriptNativeFunction] Invalid delay value type, expected Number, received " + arguments[2]);
                    }
                    delay = Math.Max((int)val, minDelay);
                }

                var taskArguments = new List<JavaScriptObject> { callerObject };
                if (arguments.Count > 3)
                {
                    for (var i = 3; i < arguments.Count; ++i)
                    {
                        taskArguments.Add(arguments[i].ToObject());
                    }
                }

                var task = new JSTaskFunction(callback, taskArguments.ToArray(), delay, repeat);
                var taskId = pushHandler(task);
  
                return JavaScriptNumber.FromInt32(taskId);
            }
            catch (InvalidOperationException ioe)
            {
                return Results.Fail("[AddScheduledJavaScriptNativeFunction] InvalidOperationException: " + ioe.Message);
            }
            catch (Exception e)
            {
                return Results.Fail("[AddScheduledJavaScriptNativeFunction] Exception: " + e.Message);
            }
        }
    }
}
