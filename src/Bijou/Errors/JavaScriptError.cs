using Bijou.Chakra;
using FluentResults;

namespace Bijou.Errors
{
    /// <summary>
    /// An exception returned from the Chakra engine.
    /// </summary>
    public class JavaScriptError : Error
    {
        /// <summary>
        /// Gets the error code.
        /// </summary>
        public JavaScriptErrorCode Code { get; }

        public JavaScriptError InnerError { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptError"/> class. 
        /// </summary>
        /// <param name="code">The error code returned.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerError">The inner error</param>
        public JavaScriptError(
            JavaScriptErrorCode code, 
            string message = null, 
            JavaScriptError innerError = null)
            : base(message)
        {
            Code = code;
            Message = message;
            InnerError = innerError;
        }
    }
}
