using Bijou.Chakra;

namespace Bijou.Errors
{
    /// <summary>
    /// An API usage exception occurred.
    /// </summary>
    public sealed class JavaScriptUsageError : JavaScriptError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptUsageError"/> class. 
        /// </summary>
        /// <param name="code">The error code returned.</param>
        /// <param name="message">The error message.</param>
        public JavaScriptUsageError(JavaScriptErrorCode code, string message)
            : base(code, message)
        {
        }
    }
}
