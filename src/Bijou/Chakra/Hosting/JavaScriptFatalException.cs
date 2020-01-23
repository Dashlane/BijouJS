using System;

namespace Bijou.Chakra.Hosting
{
    /// <summary>
    ///     A fatal exception occurred.
    /// </summary>
    internal sealed class JavaScriptFatalException : JavaScriptException
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptFatalException"/> class. 
        /// </summary>
        public JavaScriptFatalException() :
            base()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptFatalException"/> class. 
        /// </summary>
        /// <param name="message">The error message.</param>
        public JavaScriptFatalException(string message) :
            base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptFatalException"/> class. 
        /// </summary>
        /// <param name="code">The error code returned.</param>
        public JavaScriptFatalException(JavaScriptErrorCode code) :
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            this(code, "A fatal exception has occurred in a JavaScript runtime")
#pragma warning restore CA1303 // Do not pass literals as localized parameters
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptFatalException"/> class. 
        /// </summary>
        /// <param name="code">The error code returned.</param>
        /// <param name="message">The error message.</param>
        public JavaScriptFatalException(JavaScriptErrorCode code, string message) :
            base(code, message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptFatalException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        private JavaScriptFatalException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}
