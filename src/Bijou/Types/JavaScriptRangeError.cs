using Bijou.Chakra;
using FluentResults;

namespace Bijou.Types
{
    public class JavaScriptRangeError : JavaScriptObject
    {
        internal JavaScriptRangeError(JavaScriptValue value) : base(value)
        {
        }

        public static Result<JavaScriptRangeError> Create(JavaScriptString message)
        {
            return NativeMethods.JsCreateRangeError(message);
        }
    }
}