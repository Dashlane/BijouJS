﻿namespace Bijou.Chakra.Hosting
{
    /// <summary>
    /// A fatal exception occurred.
    /// </summary>
    internal sealed class JavaScriptFatalException : JavaScriptException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptFatalException"/> class. 
        /// </summary>
        /// <param name="code">The error code returned.</param>
        public JavaScriptFatalException(JavaScriptErrorCode code)
            : this(code, "A fatal exception has occurred in a JavaScript runtime")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptFatalException"/> class. 
        /// </summary>
        /// <param name="code">The error code returned.</param>
        /// <param name="message">The error message.</param>
        public JavaScriptFatalException(JavaScriptErrorCode code, string message) :
            base(code, message)
        {
        }
    }
}
