using Bijou.Chakra;
using FluentResults;

namespace Bijou.Types
{
    public class JavaScriptTypeError : JavaScriptObject
    {
        internal JavaScriptTypeError(JavaScriptValue value) : base(value)
        {
        }

        public static Result<JavaScriptTypeError> CreateTypeError(JavaScriptString message)
        {
            return NativeMethods.JsCreateTypeError(message);
        }
    }
}
