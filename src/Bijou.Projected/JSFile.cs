namespace Bijou.Projected
{
    /// <summary>
    /// Class projected to JS context, representing a file.
    /// Properties name are lowerCase as projection force them to lowerCase.
    /// </summary>
    public sealed class JSFile
    {
        /// <summary>
        /// True if the JSFile has error, false otherwise
        /// </summary>
        public bool hasError { get; internal set; } = false;

        /// <summary>
        /// Error message, empty if hasError is false 
        /// </summary>
        public string errorMessage { get; internal set; } = "";

        /// <summary>
        /// Content if the file, empty if hasError is true
        /// </summary>
        public string content { get; internal set; } = "";
    }
}
