using Bijou.Chakra;

namespace Bijou.Errors
{
    /// <summary>
    /// An exception that occurred in the workings of the JavaScript engine itself.
    /// </summary>
    public sealed class JavaScriptEngineError : JavaScriptError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptEngineError"/> class. 
        /// </summary>
        /// <param name="code">The error code returned.</param>
        /// <param name="message">The error message.</param>
        public JavaScriptEngineError(JavaScriptErrorCode code, string message) 
            : base(code, message)
        {
        }
    }
}
