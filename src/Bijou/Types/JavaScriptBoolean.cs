using Bijou.Chakra;
using FluentResults;

namespace Bijou.Types
{
    public sealed class JavaScriptBoolean : JavaScriptObject
    {
        public static JavaScriptBoolean True
        {
            get
            {
                var result = NativeMethods.JsGetTrueValue();
                return result.IsSuccess ?
                    result.Value :
                    new JavaScriptBoolean(JavaScriptValue.Invalid);
            }
        }

        public static JavaScriptBoolean False
        {
            get
            {
                var result = NativeMethods.JsGetFalseValue();
                return result.IsSuccess ?
                    result.Value :
                    new JavaScriptBoolean(JavaScriptValue.Invalid);
            }
        }

        internal JavaScriptBoolean(JavaScriptValue value) : base(value)
        {
        }

        public Result<bool> AsBool() => NativeMethods.JsBooleanToBool(this);

        public static explicit operator bool(JavaScriptBoolean value) => NativeMethods.JsBooleanToBool(value).Value;

        public static Result<JavaScriptBoolean> FromBoolean(bool value) => NativeMethods.JsBoolToBoolean(value);
    }
}
