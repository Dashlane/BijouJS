using Bijou.Chakra;
using FluentResults;

namespace Bijou.Types
{
    public static class JavaScriptObjectExtensions
    {
        public static Result<JavaScriptString> ToString(this JavaScriptObject obj)
        {
            return NativeMethods.JsConvertValueToString(obj);
        }

        public static Result<JavaScriptBoolean> ToBoolean(this JavaScriptObject obj)
        {
            return NativeMethods.JsConvertValueToBoolean(obj);
        }

        public static Result<JavaScriptNumber> ToNumber(this JavaScriptObject obj)
        {
            return NativeMethods.JsConvertValueToNumber(obj);
        }
    }
}
