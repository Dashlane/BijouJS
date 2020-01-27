using Bijou.Chakra;
using FluentResults;

namespace Bijou.Types
{
    public class JavaScriptUriError : JavaScriptObject
    {
        internal JavaScriptUriError(JavaScriptValue value) : base(value)
        {
        }

        public static Result<JavaScriptUriError> Create(JavaScriptString message)
        {
            return NativeMethods.JsCreateURIError(message);
        }
    }
}
