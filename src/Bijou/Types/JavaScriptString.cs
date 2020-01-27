using System;
using System.Linq;
using System.Runtime.InteropServices;
using Bijou.Chakra;
using FluentResults;

namespace Bijou.Types
{
    public sealed class JavaScriptString : JavaScriptObject
    {
        public int Length => NativeMethods.JsGetStringLength(this).ValueOrDefault;

        internal JavaScriptString(JavaScriptValue value) : base(value)
        {
        }

        public Result<string> AsString()
        {
            var result = NativeMethods.JsStringToPointer(UnderlyingValue);
            if (result.IsFailed)
            {
                return Results.Fail(result.Errors.First());
            }

            var (buffer, length) = result.Value;

            return Results.Ok(Marshal.PtrToStringUni(buffer, (int)length));
        }

        public static Result<JavaScriptString> FromString(string value) => 
            NativeMethods.JsPointerToString(value, new UIntPtr((uint)value.Length));

        public static explicit operator string(JavaScriptString value)
        {
            var (buffer, length) = NativeMethods.JsStringToPointer(value.UnderlyingValue).Value;

            return Marshal.PtrToStringUni(buffer, (int)length);
        }
    }
}