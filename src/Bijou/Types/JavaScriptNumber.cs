using Bijou.Chakra;
using FluentResults;

namespace Bijou.Types
{
    public sealed class JavaScriptNumber : JavaScriptObject
    {
        internal JavaScriptNumber(JavaScriptValue value) : base(value)
        {
        }

        public Result<double> AsDouble() => NativeMethods.JsNumberToDouble(this);

        public Result<int> AsInt32() => NativeMethods.JsNumberToInt(this);

        public static Result<JavaScriptNumber> FromDouble(double value) => NativeMethods.JsDoubleToNumber(value);

        public static Result<JavaScriptNumber> FromInt32(int value) => NativeMethods.JsIntToNumber(value);

        public static explicit operator double(JavaScriptNumber obj) => NativeMethods.JsNumberToDouble(obj).Value;

        public static explicit operator int(JavaScriptNumber obj) => NativeMethods.JsNumberToInt(obj).Value;
    }
}