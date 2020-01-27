using Bijou.Chakra;
using FluentResults;

namespace Bijou.Types
{
    public class JavaScriptError : JavaScriptObject
    {
        internal JavaScriptError(JavaScriptValue value) : base(value)
        {
        }
        public static Result<JavaScriptError> Create(JavaScriptString message)
        {
            return NativeMethods.JsCreateError(message);
        }
    }
}
