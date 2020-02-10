using System.Diagnostics;
using System.Linq;
using Bijou.Errors;
using FluentResults;
using JavaScriptError = Bijou.Errors.JavaScriptError;

namespace Bijou.Chakra
{
    internal static class JavaScriptErrorCodeExtensions
    {
        public static Result ToResult(this JavaScriptErrorCode code)
        {
            return code == JavaScriptErrorCode.NoError ? Results.Ok()
                                                       : Results.Fail(MapErrorCode(code));
        }

        public static Result<JavaScriptValue> ToResult(this JavaScriptErrorCode code, JavaScriptValue value)
        {
            return code == JavaScriptErrorCode.NoError ? Results.Ok(value)
                                                       : Results.Fail<JavaScriptValue>(MapErrorCode(code));
        }

        public static Result<T> ToResult<T>(this JavaScriptErrorCode code, T value)
        {
            return code == JavaScriptErrorCode.NoError ? Results.Ok(value)
                                                       : Results.Fail<T>(MapErrorCode(code));
        }

        private static JavaScriptError MapErrorCode(JavaScriptErrorCode error)
        {
            switch (error)
            {
                case JavaScriptErrorCode.InvalidArgument:
                    return new JavaScriptUsageError(error, "Invalid argument.");

                case JavaScriptErrorCode.NullArgument:
                    return new JavaScriptUsageError(error, "Null argument.");

                case JavaScriptErrorCode.NoCurrentContext:
                    return new JavaScriptUsageError(error, "No current context.");

                case JavaScriptErrorCode.InExceptionState:
                    return new JavaScriptUsageError(error, "Runtime is in exception state.");

                case JavaScriptErrorCode.NotImplemented:
                    return new JavaScriptUsageError(error, "Method is not implemented.");

                case JavaScriptErrorCode.WrongThread:
                    return new JavaScriptUsageError(error, "Runtime is active on another thread.");

                case JavaScriptErrorCode.RuntimeInUse:
                    return new JavaScriptUsageError(error, "Runtime is in use.");

                case JavaScriptErrorCode.BadSerializedScript:
                    return new JavaScriptUsageError(error, "Bad serialized script.");

                case JavaScriptErrorCode.InDisabledState:
                    return new JavaScriptUsageError(error, "Runtime is disabled.");

                case JavaScriptErrorCode.CannotDisableExecution:
                    return new JavaScriptUsageError(error, "Cannot disable execution.");

                case JavaScriptErrorCode.AlreadyDebuggingContext:
                    return new JavaScriptUsageError(error, "Context is already in debug mode.");

                case JavaScriptErrorCode.HeapEnumInProgress:
                    return new JavaScriptUsageError(error, "Heap enumeration is in progress.");

                case JavaScriptErrorCode.ArgumentNotObject:
                    return new JavaScriptUsageError(error, "Argument is not an object.");

                case JavaScriptErrorCode.InProfileCallback:
                    return new JavaScriptUsageError(error, "In a profile callback.");

                case JavaScriptErrorCode.InThreadServiceCallback:
                    return new JavaScriptUsageError(error, "In a thread service callback.");

                case JavaScriptErrorCode.CannotSerializeDebugScript:
                    return new JavaScriptUsageError(error, "Cannot serialize a debug script.");

                case JavaScriptErrorCode.AlreadyProfilingContext:
                    return new JavaScriptUsageError(error, "Already profiling this context.");

                case JavaScriptErrorCode.IdleNotEnabled:
                    return new JavaScriptUsageError(error, "Idle is not enabled.");

                case JavaScriptErrorCode.OutOfMemory:
                    return new JavaScriptEngineError(error, "Out of memory.");

                case JavaScriptErrorCode.ScriptException:
                    {
                        var innerError = NativeMethods.JsGetAndClearException();
                        if (innerError.IsFailed)
                        {
                            var jsError = (JavaScriptError)innerError.Errors.First();
                            return new JavaScriptFatalError(jsError.Code);
                        }

                        return new JavaScriptScriptError(error, innerError.Value, "Script threw an exception.");
                    }

                case JavaScriptErrorCode.ScriptCompile:
                    {
                        var innerError = NativeMethods.JsGetAndClearException();
                        if (innerError.IsFailed)
                        {
                            var jsError = (JavaScriptError)innerError.Errors.First();
                            return new JavaScriptFatalError(jsError.Code);
                        }

                        return new JavaScriptScriptError(error, innerError.Value, "Compile error.");
                    }

                case JavaScriptErrorCode.ScriptTerminated:
                    return new JavaScriptScriptError(error, JavaScriptValue.Invalid, "Script was terminated.");

                case JavaScriptErrorCode.ScriptEvalDisabled:
                    return new JavaScriptScriptError(error, JavaScriptValue.Invalid, "Eval of strings is disabled in this runtime.");

                case JavaScriptErrorCode.Fatal:
                    return new JavaScriptFatalError(error);

                default:
                    return new JavaScriptFatalError(error);
            }
        }
    }
}
