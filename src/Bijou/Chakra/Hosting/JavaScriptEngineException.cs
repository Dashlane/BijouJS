﻿namespace Bijou.Chakra.Hosting
{
    /// <summary>
    /// An exception that occurred in the workings of the JavaScript engine itself.
    /// </summary>
    internal sealed class JavaScriptEngineException : JavaScriptException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptEngineException"/> class. 
        /// </summary>
        /// <param name="code">The error code returned.</param>
        /// <param name="message">The error message.</param>
        public JavaScriptEngineException(JavaScriptErrorCode code, string message) 
            : base(code, message)
        {
        }
    }
}
