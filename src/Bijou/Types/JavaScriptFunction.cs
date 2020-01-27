using System;
using Bijou.Chakra;
using FluentResults;

namespace Bijou.Types
{
    public sealed class JavaScriptFunction : JavaScriptPrototype
    {
        internal JavaScriptFunction(JavaScriptValue value) : base(value)
        {
        }

        public Result<TValue> CallFunction<TValue>(params JavaScriptObject[] arguments)
            where TValue : JavaScriptObject
        {
            if (arguments.Length > ushort.MaxValue)
            {
                return Results.Fail("Arguments out of range");
            }

            var result = NativeMethods.JsCallFunction<TValue>(this, arguments, (ushort)arguments.Length);
            if (result.IsFailed)
            {
                return Results.Fail($"An error occurred when executing function {UnderlyingValue}");
            }

            return Results.Ok((TValue) result.Value);
        }

        public Result<JavaScriptObject> ConstructObject(params JavaScriptObject[] arguments)
        {
            if (arguments.Length > ushort.MaxValue)
            {
                return Results.Fail("Arguments out of range");
            }

            return NativeMethods.JsConstructObject(this, arguments, (ushort)arguments.Length);
        }

        internal static Result<JavaScriptFunction> CreateFunction(JavaScriptNativeFunction function)
        {
            return NativeMethods.JsCreateFunction(function, IntPtr.Zero);
        }

        internal static Result<JavaScriptFunction> CreateFunction(JavaScriptNativeFunction function, IntPtr callbackData)
        {
            return NativeMethods.JsCreateFunction(function, callbackData);
        }
    }
}
