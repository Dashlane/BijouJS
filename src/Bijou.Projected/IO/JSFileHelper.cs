using System.Threading.Tasks;

namespace Bijou.Projected.IO
{
    /// <summary>
    /// JSFileHelper implements thread-safe filesystem read/write access.
    /// Thread-safety is provided by a dictionary of semaphores, where each
    /// file has a dedicated semaphore. 
    /// The dictionary is thread-safe protected by a dedicated semaphore.
    /// We do not use a thread-safe container as we can't have thread-safety
    /// when adding/disposing a file's semaphore.
    /// </summary>
    internal static class JSFileHelper
    {
        /// <summary>
        /// Async check if the given relative file corresponds to an existing file in local folder. 
        /// </summary>
        /// <param name="filePath">Relative path to a file</param>
        /// <returns>True if file exists, false otherwise</returns>
        internal static async Task<bool> Exists(string filePath)
        {
            return await FileSystem.ExistsAsync(filePath);
        }

        /// <summary>
        /// Async method to read a file in the UWP app's local folder
        /// </summary>
        /// <param name="filePath">Relative path to a file in UWP app's local folder</param>
        /// <returns>A JSFile, containing file's content if file exists and it is readable,
        /// with hasError set to true in case of error</returns>
        internal static async Task<JSFile> Read(string filePath)
        {
            var file = await FileSystem.ReadAsync(filePath);
            return new JSFile() {
                content = file.Content,
                hasError = file.HasError,
                errorMessage = file.ErrorMessage
            };
        }

        /// <summary>
        /// Async method to write a file in UWP app's local folder
        /// </summary>
        /// <param name="filePath">Relative path to a file in UWP app's local folder</param>
        /// <param name="fileContent">File text content to write</param>
        /// <returns>True if the operation succeds, false otherwise</returns>
        internal static async Task<bool> Write(string filePath, string fileContent)
        {
            return await FileSystem.WriteAsync(filePath, fileContent);
        }

        /// <summary>
        /// Async method to remove a file from UWP app's local folder
        /// </summary>
        /// <param name="filePath">Relative path to a file in UWP app's local folder</param>
        /// <returns>True if the operation succeds, false otherwise</returns>
        internal static async Task<string> Remove(string filePath)
        {
            return await FileSystem.RemoveAsync(filePath);
        }
    }
}
