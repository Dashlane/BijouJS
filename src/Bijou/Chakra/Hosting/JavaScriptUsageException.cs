using System;

namespace Bijou.Chakra.Hosting
{
    /// <summary>
    ///     An API usage exception occurred.
    /// </summary>
    internal sealed class JavaScriptUsageException : JavaScriptException
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptUsageException"/> class. 
        /// </summary>
        public JavaScriptUsageException() :
            base()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptUsageException"/> class. 
        /// </summary>
        /// <param name="message">The error message.</param>
        public JavaScriptUsageException(string message) :
            base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptUsageException"/> class. 
        /// </summary>
        /// <param name="code">The error code returned.</param>
        public JavaScriptUsageException(JavaScriptErrorCode code) :
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            this(code, "A fatal exception has occurred in a JavaScript runtime")
#pragma warning restore CA1303 // Do not pass literals as localized parameters
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptUsageException"/> class. 
        /// </summary>
        /// <param name="code">The error code returned.</param>
        /// <param name="message">The error message.</param>
        public JavaScriptUsageException(JavaScriptErrorCode code, string message) :
            base(code, message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptUsageException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        private JavaScriptUsageException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}
