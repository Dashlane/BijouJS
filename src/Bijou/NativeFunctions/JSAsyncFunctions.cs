using System;
using System.Collections.Generic;
using Bijou.Chakra.Hosting;
using Bijou.Executor;
using Bijou.JSTasks;

namespace Bijou.NativeFunctions
{
    internal static class JSAsyncFunctions
    {
        /// <summary>
        ///     JS Native function for setTimeout
        /// </summary>
        static public JavaScriptValue SetTimeoutJavaScriptNativeFunction(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            // Syntax
            // setTimeout(function, milliseconds, param1, param2, ...)
            // Parameter Values
            // function:            Required. The function to be executed
            // milliseconds:        Optional. The number of milliseconds to wait before executing the code. If omitted, the value 0 is used
            // param1, param2, ...:	Optional. Additional parameters to pass to the function
            // Return Value
            // A Number, representing the ID value of the timer that is set.Use this value with the clearInterval() method to cancel the timer


            // check arguments
            if (argumentCount < 2 || arguments.Length < 2) {
                Console.Error.WriteLine("[SetTimeoutJavaScriptNativeFunction] Invalid argumentCount, expected >= 2, received " + argumentCount);
                return JavaScriptValue.Invalid;
            }

            return AddScheduledJavaScriptNativeFunction(arguments, callbackData, 0, false);
        }

        /// <summary>
        ///     JS Native function for setInterval
        /// </summary>
        static public JavaScriptValue SetIntervalJavaScriptNativeFunction(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            // Syntax
            // setInterval(function, milliseconds, param1, param2, ...)
            // Parameter Values
            // function:            Required. The function to be executed
            // milliseconds:        Required. The intervals (in milliseconds) on how often to execute the code. If the value is less than 10, the value 10 is used
            // param1, param2, ...:	Optional. Additional parameters to pass to the function
            // Return Value
            // A Number, representing the ID value of the timer that is set.Use this value with the clearInterval() method to cancel the timer

            if (argumentCount < 3 || arguments.Length < 3) {
              Console.Error.WriteLine("[SetIntervalJavaScriptNativeFunction] Invalid argumentCount, expected >= 3, received " + argumentCount);
                return JavaScriptValue.Invalid;
            }

            return AddScheduledJavaScriptNativeFunction(arguments, callbackData, 10, true);
        }

        /// <summary>
        ///     JS Native function for generic async operation (setTimeout / setInterval)
        /// </summary>
        static private JavaScriptValue AddScheduledJavaScriptNativeFunction(JavaScriptValue[] arguments, IntPtr callbackData, int minDelay, bool repeat = false)
        {
            var ret = JavaScriptValue.Invalid;
            try {
                UWPChakraHostExecutor executor = JSHelpers.ExecutorFromCallbackData(callbackData);

                if (executor == null) {
                    Console.Error.WriteLine("[AddScheduledJavaScriptNativeFunction] Invalid executor");
                    return ret;
                }

                if (arguments.Length < 2) {
                    Console.Error.WriteLine("[AddScheduledJavaScriptNativeFunction] Invalid argumentCount, expected >= 2, received " + arguments.Length);
                    return ret;
                }

                // arguments[0] is JavaScript this
                JavaScriptValue callerObject = arguments[0];

                // setTimeout / setInterval signature is (callback, [after, params...])
                JavaScriptValue callback = arguments[1];

                var delay = minDelay;
                if (arguments.Length > 2) {
                    JavaScriptValue afterValue = arguments[2];
                    if (!afterValue.IsValid) {
                        Console.Error.WriteLine("[AddScheduledJavaScriptNativeFunction] Invalid delay argument, expected Number, received Invalid");
                        return ret;
                    }
                    if (afterValue.ValueType != JavaScriptValueType.Number) {
                        Console.Error.WriteLine("[AddScheduledJavaScriptNativeFunction] Invalid delay value type, expected Number, received " + arguments[2].ValueType);
                        return ret;
                    }
                    delay = Math.Max(afterValue.ToInt32(), minDelay);
                }

                List<JavaScriptValue> taskArguments = new List<JavaScriptValue> { callerObject };
                if (arguments.Length > 3) {
                    for (int i = 3 ; i < arguments.Length ; ++i) {
                        taskArguments.Add(arguments[i]);
                    }
                }

                var task = new JSTaskFunction(callback, taskArguments.ToArray(), (int)delay, repeat);
                // add task to scheduled tasks
                int taskId = executor.AddCancellableTask(task);
                // return task ID     
                ret = JavaScriptValue.FromInt32(taskId);
            } catch (InvalidOperationException ioe) {
                Console.Error.WriteLine("[AddScheduledJavaScriptNativeFunction] InvalidOperationException: " + ioe.Message);
            } catch (Exception e) {
                Console.Error.WriteLine("[AddScheduledJavaScriptNativeFunction] Exception: " + e.Message);
                throw;
            }

            return ret;
        }

        /// <summary>
        ///     JS Native function for clear async operation (clearTimeout / clearInterval)
        /// </summary>
        static public JavaScriptValue ClearScheduledJavaScriptNativeFunction(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            // Syntax
            // clearTimeout/clearInterval(var)
            // Parameter Values
            // var:  Required. The ID of the timer returned by the setTimeout/setInterval() method
            // Return Value
            // No return value
            var ret = JavaScriptValue.Invalid;

            // check arguments
            if (argumentCount != 2 || arguments.Length != 2) {
                Console.Error.WriteLine("[ClearScheduledJavaScriptNativeFunction] Invalid argumentCount, expected  2, received " + arguments.Length);
                return ret;
            }

            UWPChakraHostExecutor executor = JSHelpers.ExecutorFromCallbackData(callbackData);
            if (executor == null) {
                Console.Error.WriteLine("[ClearScheduledJavaScriptNativeFunction] Invalid executor");
                return ret;
            }

            // arguments[0] is JavaScript this
            JavaScriptValue callerObject = arguments[0];
            // clearTimeout / clearInterval signature is (id)
            JavaScriptValue id = arguments[1];

            // argument validation
            if (!id.IsValid) {
                Console.Error.WriteLine("[ClearScheduledJavaScriptNativeFunction] Invalid argument");
                return ret;
            }

            if (id.ValueType != JavaScriptValueType.Number) {
                Console.Error.WriteLine("[ClearScheduledJavaScriptNativeFunction] Invalid argument type, expected Number, received " + id.ValueType);
                return ret;
            }

            int taskId = id.ToInt32();
            executor.CancelTask(taskId);

            return ret;
        }

        /// <summary>
        ///     JS Native function to use as a callback for ES6 promises
        /// </summary>
        public static void PromiseContinuationCallback(JavaScriptValue promise, IntPtr callbackData)
        {
            UWPChakraHostExecutor executor = JSHelpers.ExecutorFromCallbackData(callbackData);

            if (executor == null) {
                Console.Error.WriteLine("[PromiseContinuationCallback] Error parsing callbackData");
                return;
            } else if (!JavaScriptContext.IsCurrentValid) {
                Console.Error.WriteLine("[PromiseContinuationCallback] Invalid Context");
                return;
            }

            JavaScriptValue globalObject = JavaScriptValue.GlobalObject;

            var task = new JSTaskFunction(promise, new JavaScriptValue[1] { globalObject });
            task.IsPromise = true;

            executor.AddTask(task);
        }
    }
}
