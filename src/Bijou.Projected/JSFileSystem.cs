using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Bijou.Projected
{
    /// <summary>
    /// Class projected to JS context, implementing file system async feature.
    /// Methods name are lowerCase as projection force them to lowerCase
    /// As Task are not WinRT component, in order to have exposed async methods
    /// we need to return IAsyncOperation.In order to return IAsyncOperation,
    /// we return a Task as an Async Operation.
    /// </summary>
    public static class JSFileSystem
    {
        /// <summary>
        /// JS Native async function to check if a file exists in the UWP app's local folder
        /// </summary>
        /// <param name="filePath">Relative path to a file in UWP app's local folder</param>
        /// <returns>True if the file exists, false otherwise</returns>
        public static IAsyncOperation<bool> existsAsync(string filePath)
        {
            var fileExistsTask = Task.FromResult(false).AsAsyncOperation();
            try {
                fileExistsTask = JSFileHelper.Exists(filePath).AsAsyncOperation();
#pragma warning disable CA1031 // Do not catch general exception types
            } catch (Exception) { }
#pragma warning restore CA1031 // Do not catch general exception types
            return fileExistsTask;
        }

        /// <summary>
        /// JS Native async function to read a file in the UWP app's local folder
        /// </summary>
        /// <param name="filePath">Relative path to a file in UWP app's local folder</param>
        /// <returns>A JSFile, containing file's content if file exists and it is readable,
        /// with hasError set to true in case of error</returns>
        public static IAsyncOperation<JSFile> readAsync(string filePath)
        {
            var fileContentTask = Task.FromResult(new JSFile()).AsAsyncOperation();
            try {
                fileContentTask = JSFileHelper.Read(filePath).AsAsyncOperation();
#pragma warning disable CA1031 // Do not catch general exception types
            } catch (Exception) { }
#pragma warning restore CA1031 // Do not catch general exception types
            return fileContentTask;
        }

        /// <summary>
        /// JS Native async function to write to a file in the UWP app's local folder
        /// </summary>
        /// <param name="filePath">Relative path to a file in UWP app's local folder</param>
        /// <param name="fileContent">File content to write to memory</param>
        /// <returns>True in case of success, false otherwise</returns>
        public static IAsyncOperation<bool> writeAsync(string filePath, string fileContent)
        {
            var writeTask = Task.FromResult(false).AsAsyncOperation();
            try {
                writeTask = JSFileHelper.Write(filePath, fileContent).AsAsyncOperation();
#pragma warning disable CA1031 // Do not catch general exception types
            } catch (Exception) { }
#pragma warning restore CA1031 // Do not catch general exception types
            return writeTask;
        }

        /// <summary>
        /// JS Native async method to remove a file from UWP app's local folder
        /// </summary>
        /// <param name="filePath">Relative path to a file in UWP app's local folder</param>
        /// <returns>True if the operation succeds, false otherwise</returns>
        public static IAsyncOperation<string> removeAsync(string filePath)
        {
            var removeTask = Task.FromResult("false").AsAsyncOperation();
            try {
                removeTask = JSFileHelper.Remove(filePath).AsAsyncOperation();
#pragma warning disable CA1031 // Do not catch general exception types
            } catch (Exception) { }
#pragma warning restore CA1031 // Do not catch general exception types
            return removeTask;
        }
    }
}
