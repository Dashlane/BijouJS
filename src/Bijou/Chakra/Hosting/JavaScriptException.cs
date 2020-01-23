using System;

namespace Bijou.Chakra.Hosting
{
    /// <summary>
    /// An exception returned from the Chakra engine.
    /// </summary>
    public class JavaScriptException : Exception
    {
        /// <summary>
        /// Gets the error code.
        /// </summary>
        public JavaScriptErrorCode ErrorCode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptException"/> class. 
        /// </summary>
        public JavaScriptException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptException"/> class. 
        /// </summary>
        /// <param name="message">The error message.</param>
        public JavaScriptException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptException"/> class. 
        /// </summary>
        /// <param name="code">The error code returned.</param>
        /// <param name="message">The error message.</param>
        public JavaScriptException(JavaScriptErrorCode code, string message)
            : base(message)
        {
            ErrorCode = code;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptException"/> class. 
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        protected JavaScriptException(string message, Exception innerException)
            : base(message, innerException)
        {
            if (message != null) 
            {
                ErrorCode = (JavaScriptErrorCode)HResult;
            }
        }
    }
}
