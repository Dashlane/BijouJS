namespace Bijou.Chakra.Hosting
{
    /// <summary>
    ///     A script exception.
    /// </summary>
    internal sealed class JavaScriptScriptException : JavaScriptException
    {
        /// <summary>
        ///     Gets a JavaScript object representing the script error.
        /// </summary>
        public JavaScriptValue Error { get; }

        /// <summary>
        ///     Gets a string representing the script error message.
        /// </summary>
        public string ErrorMessage => GetErrorProperty("message");

        /// <summary>
        ///     Gets a string representing the script error filename.
        /// </summary>
        public string ErrorFileName => GetErrorProperty("filename");

        /// <summary>
        ///     Gets a string representing the script error lineNumber.
        /// </summary>
        public string ErrorLineNumber => GetErrorProperty("lineNumber");

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptScriptException"/> class. 
        /// </summary>
        /// <param name="code">The error code returned.</param>
        /// <param name="error">The JavaScript error object.</param>
        /// <param name="message">The error message.</param>
        public JavaScriptScriptException(JavaScriptErrorCode code, JavaScriptValue error, string message)
            : base(code, message)
        {
            Error = error;
        }

        private string GetErrorProperty(string propertyName)
        {
            if (!Error.IsValid)
            {
                return string.Empty;
            }

            NativeMethods.ThrowIfError(NativeMethods.JsGetPropertyIdFromName(propertyName, out var errorMessage));

            NativeMethods.ThrowIfError(NativeMethods.JsGetProperty(Error, errorMessage, out var messageValue));

            if (messageValue.IsValid && messageValue.ValueType == JavaScriptValueType.String) 
            {
                return messageValue.ToString();
            }

            return string.Empty;
        }
    }
}
