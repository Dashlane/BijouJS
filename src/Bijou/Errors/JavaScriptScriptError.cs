using Bijou.Chakra;
using Bijou.Types;

namespace Bijou.Errors
{
    /// <summary>
    ///     A script exception.
    /// </summary>
    public sealed class JavaScriptScriptError : JavaScriptError
    {
        private const string ErrorProperty = "message";
        private const string FileNameProperty = "filename";
        private const string LineNumberProperty = "lineNumber";

        /// <summary>
        ///     Gets a string representing the script error message.
        /// </summary>
        public string Error { get; }

        /// <summary>
        ///     Gets a string representing the script error filename.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        ///     Gets a string representing the script error lineNumber.
        /// </summary>
        public string LineNumber { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptScriptError"/> class. 
        /// </summary>
        /// <param name="code">The error code returned.</param>
        /// <param name="error">The JavaScript error object.</param>
        /// <param name="message">The error message.</param>
        public JavaScriptScriptError(
            JavaScriptErrorCode code, 
            JavaScriptValue error, 
            string message)
            : base(code, message)
        {
            Error = GetErrorProperty(ErrorProperty, error);
            FileName = GetErrorProperty(FileNameProperty, error);
            LineNumber = GetErrorProperty(LineNumberProperty, error);
        }

        private static string GetErrorProperty(string propertyName, JavaScriptValue error)
        {
            if (!error.IsValid)
            {
                return string.Empty;
            }

            var errorMessage = NativeMethods.JsGetPropertyIdFromName(propertyName);
            if (errorMessage.IsFailed)
            {
                return $"Failed to found property {propertyName}";
            }

            var messageValue = NativeMethods.JsGetProperty<JavaScriptString>(error, errorMessage.Value);
            if (messageValue.IsFailed)
            {
                return $"Failed to extract property {propertyName}";
            }

            return messageValue.Value.IsValid ?
                messageValue.Value.AsString().ValueOrDefault : 
                string.Empty;
        }
    }
}
