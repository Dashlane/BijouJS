using Bijou.Chakra;

namespace Bijou.Errors
{
    /// <summary>
    /// A fatal exception occurred.
    /// </summary>
    public sealed class JavaScriptFatalError : JavaScriptError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptFatalError"/> class. 
        /// </summary>
        /// <param name="code">The error code returned.</param>
        public JavaScriptFatalError(JavaScriptErrorCode code)
            : this(code, "A fatal exception has occurred in a JavaScript runtime")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptFatalError"/> class. 
        /// </summary>
        /// <param name="code">The error code returned.</param>
        /// <param name="message">The error message.</param>
        public JavaScriptFatalError(JavaScriptErrorCode code, string message) :
            base(code, message)
        {
        }
    }
}
