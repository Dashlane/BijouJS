namespace Bijou.Projected.IO
{
    public class File
    {
        /// <summary>
        /// True if the File has error, false otherwise
        /// </summary>
        public bool HasError { get; internal set; } = false;

        /// <summary>
        /// Error message, empty if hasError is false 
        /// </summary>
        public string ErrorMessage { get; internal set; } = "";

        /// <summary>
        /// Content if the file, empty if hasError is true
        /// </summary>
        public string Content { get; internal set; } = "";
    }
}
