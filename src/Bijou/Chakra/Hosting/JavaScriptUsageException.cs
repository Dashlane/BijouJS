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
        /// <param name="code">The error code returned.</param>
        /// <param name="message">The error message.</param>
        public JavaScriptUsageException(JavaScriptErrorCode code, string message)
            : base(code, message)
        {
        }
    }
}
