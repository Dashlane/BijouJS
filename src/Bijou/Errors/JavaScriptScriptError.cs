using Bijou.Chakra;

namespace Bijou.Errors
{
    /// <summary>
    /// A script exception.
    /// </summary>
    public sealed class JavaScriptScriptError : JavaScriptError
    {
        private const string ErrorProperty = "message";
        private const string FileNameProperty = "filename";
        private const string LineNumberProperty = "lineNumber";

        /// <summary>
        /// Gets a string representing the script error message.
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// Gets a string representing the script error filename.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets a string representing the script error lineNumber.
        /// </summary>
        public string LineNumber { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptScriptError"/> class. 
        /// </summary>
        /// <param name="code">The error code returned.</param>
        /// <param name="error">The JavaScript error object.</param>
        /// <param name="message">The error message.</param>
        internal JavaScriptScriptError(
            JavaScriptErrorCode code,
            JavaScriptValue error,
            string message)
            : base(code, message)
        {
            Error = GetErrorProperty(ErrorProperty, error);
            FileName = GetErrorProperty(FileNameProperty, error);
            LineNumber = GetErrorProperty(LineNumberProperty, error);
        }

        /// <summary>
        /// Gets the error object's property as a string.
        /// </summary>
        /// <param name="propertyName">Property to get</param>
        /// <param name="error">The error object to get the property from</param>
        /// <returns>The property as a string</returns>
        private static string GetErrorProperty(string propertyName, JavaScriptValue error)
        {
            if (!error.IsValid)
            {
                return string.Empty;
            }

            var resultId = NativeMethods.JsGetPropertyIdFromName(propertyName);
            if (resultId.IsFailed)
            {
                return $"Failed to found property {propertyName}";
            }

            var resultProperty = NativeMethods.JsGetProperty(error, resultId.Value);
            if (resultProperty.IsFailed)
            {
                return $"Failed to extract property {propertyName}";
            }

            return resultProperty.Value.ToString();
        }
    }
}
