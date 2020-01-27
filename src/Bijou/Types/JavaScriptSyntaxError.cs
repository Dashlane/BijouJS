using Bijou.Chakra;
using FluentResults;

namespace Bijou.Types
{
    public class JavaScriptSyntaxError : JavaScriptObject
    {
        internal JavaScriptSyntaxError(JavaScriptValue value) : base(value)
        {
        }

        public static Result<JavaScriptSyntaxError> CreateSyntaxError(JavaScriptString message)
        {
            return NativeMethods.JsCreateSyntaxError(message);
        }
    }
}
