using Bijou.Chakra;
using FluentResults;

namespace Bijou.Types
{
    public class JavaScriptReferenceError : JavaScriptObject
    {
        internal JavaScriptReferenceError(JavaScriptValue value) : base(value)
        {
        }

        public static Result<JavaScriptReferenceError> CreateReferenceError(JavaScriptString message)
        {
            return NativeMethods.JsCreateReferenceError(message);
        }
    }
}
