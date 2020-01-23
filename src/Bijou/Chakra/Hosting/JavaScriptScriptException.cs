using System;

namespace Bijou.Chakra.Hosting
{
    /// <summary>
    ///     A script exception.
    /// </summary>
    internal sealed class JavaScriptScriptException : JavaScriptException
    {
        /// <summary>
        /// The error.
        /// </summary>
        private readonly JavaScriptValue error;

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptScriptException"/> class. 
        /// </summary>
        public JavaScriptScriptException() :
            base()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptScriptException"/> class. 
        /// </summary>
        /// <param name="message">The error message.</param>
        public JavaScriptScriptException(string message) :
            base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptScriptException"/> class. 
        /// </summary>
        /// <param name="code">The error code returned.</param>
        /// <param name="error">The JavaScript error object.</param>
        public JavaScriptScriptException(JavaScriptErrorCode code, JavaScriptValue error) :
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            this(code, error, "JavaScript Exception")
#pragma warning restore CA1303 // Do not pass literals as localized parameters
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptScriptException"/> class. 
        /// </summary>
        /// <param name="code">The error code returned.</param>
        /// <param name="error">The JavaScript error object.</param>
        /// <param name="message">The error message.</param>
        public JavaScriptScriptException(JavaScriptErrorCode code, JavaScriptValue error, string message) :
            base(code, message)
        {
            this.error = error;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaScriptScriptException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        private JavaScriptScriptException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        /// <summary>
        ///     Gets a JavaScript object representing the script error.
        /// </summary>
        public JavaScriptValue Error
        {
            get
            {
                return error;
            }
        }

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

        private string GetErrorProperty(string propertyName)
        {
            if (Error.IsValid) {
                JavaScriptPropertyId errorMessage;
                NativeMethods.ThrowIfError(NativeMethods.JsGetPropertyIdFromName(propertyName, out errorMessage));

                JavaScriptValue messageValue;
                NativeMethods.ThrowIfError(NativeMethods.JsGetProperty(Error, errorMessage, out messageValue));

                if (messageValue.IsValid && messageValue.ValueType == JavaScriptValueType.String) {
                    return messageValue.ToString();
                }
            }
            return "";
        }
    }
}
