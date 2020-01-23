using System;

namespace Bijou.Chakra.Hosting
{
    /// <summary>
    ///     An exception that occurred in the workings of the JavaScript engine itself.
    /// </summary>
    internal sealed class JavaScriptEngineException : JavaScriptException
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptEngineException"/> class. 
        /// </summary>
        public JavaScriptEngineException() :
            base()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptEngineException"/> class. 
        /// </summary>
        /// <param name="message">The error message.</param>
        public JavaScriptEngineException(string message) :
            base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptEngineException"/> class. 
        /// </summary>
        /// <param name="code">The error code returned.</param>
        public JavaScriptEngineException(JavaScriptErrorCode code) :
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            this(code, "A fatal exception has occurred in a JavaScript runtime")
#pragma warning restore CA1303 // Do not pass literals as localized parameters
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptEngineException"/> class. 
        /// </summary>
        /// <param name="code">The error code returned.</param>
        /// <param name="message">The error message.</param>
        public JavaScriptEngineException(JavaScriptErrorCode code, string message) :
            base(code, message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptEngineException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        private JavaScriptEngineException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}
