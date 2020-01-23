// FileSystem
/// <summary>
/// FileSystem implements thread-safe filesystem read/write access
/// Thread-safety is provided by a dictionary of semaphores, where
/// each file has a dedicated semaphore. 
/// The dictionary is thread-safe protected by a dedicated semaphore.
/// We do not use a thread-safe container as we can't have thread-safety when adding/disposing 
/// a file's semaphore
/// </summary>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Bijou.Projected.IO
{
    public static class FileSystem
    {
        // max concurent access requests
        private const int MAX_CONCURRENT_REQUEST = 1;
        // semaphore to synchronize access on _fileSystemSemaphores
        private static SemaphoreSlim _dictionarySemaphore = new SemaphoreSlim(MAX_CONCURRENT_REQUEST, MAX_CONCURRENT_REQUEST);
        // dictionary of files semaphores
        private static Dictionary<string, SemaphoreSlim> _fileSystemSemaphores = new Dictionary<string, SemaphoreSlim>();

        /// <summary>
        /// Async check if the given relative file corresponds to an existing file in local folder. 
        /// </summary>
        /// <param name="filePath">Relative path to a file</param>
        /// <returns>True if file exists, false otherwise</returns>
        public static async Task<bool> ExistsAsync(string filePath)
        {
            return await ExistsAsync(ApplicationData.Current.LocalFolder, filePath);
        }

        /// <summary>
        /// Async check if the given relative file under the given StorageFolder corresponds to an existing file in local folder. 
        /// </summary>
        /// <param name="storageFolder">StorageFolder where to look for the file</param>
        /// <param name="filePath">Relative path to a file</param>
        /// <returns></returns>
        public static async Task<bool> ExistsAsync(StorageFolder storageFolder, string filePath)
        {
            bool ret = false;
            // validate argument
            if (storageFolder == null) {
                throw new ArgumentNullException(nameof(storageFolder));
            }
            // Get file
            try {
                await LockFileAsync(filePath);
                await storageFolder.GetFileAsync(filePath);
                ret = true;
            } catch (FileNotFoundException) {
                // File does not exists
                Debug.WriteLine($"ExistsAsync - [FileNotFoundException] path : {filePath}");
            } catch (UnauthorizedAccessException) {
                // The path cannot be in Uri format
                Debug.WriteLine($"ExistsAsync - [UnauthorizedAccessException] invalid path : {filePath}");
            } catch (ArgumentException) {
                // You don't have permission to access the specified file.
            } catch (Exception e) {
                Debug.WriteLine("[Exist] " + e.Message);
                throw;
            } finally {
                // release file's semaphore
                await UnlockFileAsync(filePath);
            }
            return ret;
        }
        
        /// <summary>
        /// Async method to read a file in the UWP app's local folder
        /// </summary>
        /// <param name="filePath">Relative path to a file in UWP app's local folder</param>
        /// <returns>A File, containing file's content if file exists and it is readable,
        /// with hasError set to true in case of error</returns>
        public static async Task<File> ReadAsync(string filePath)
        {
            return await ReadAsync(ApplicationData.Current.LocalFolder, filePath);
        }

        /// <summary>
        /// Async method to read a file in the given StorageFolder
        /// </summary>
        /// <param name="storageFolder">StorageFolder where to look for the file</param>
        /// <param name="filePath">Relative path to a file in the given StorageFolder</param>
        /// <returns></returns>
        public static async Task<File> ReadAsync(StorageFolder storageFolder, string filePath)
        {
            // validate argument
            if (storageFolder == null) {
                throw new ArgumentNullException(nameof(storageFolder));
            }

            var ret = new File();
            try {
                await LockFileAsync(filePath);
                // Get file
                var readFile = await storageFolder.GetFileAsync(filePath);
                // Read text
                ret.Content = await FileIO.ReadTextAsync(readFile);
            } catch (FileNotFoundException ffe) {
                // file does not exists
                ret.HasError = true;
                ret.ErrorMessage = ffe.Message;
            } catch (UnauthorizedAccessException uae) {
                // You don't have permission to access the specified file.
                ret.HasError = true;
                ret.ErrorMessage = uae.Message;
            } catch (Exception) {
                throw;
            } finally {
                // release file's semaphore
                await UnlockFileAsync(filePath);
            }
            return ret;
        }

        /// <summary>
        /// Async method to write a file in UWP app's local folder
        /// </summary>
        /// <param name="filePath">Relative path to a file in UWP app's local folder</param>
        /// <param name="fileContent">File text content to write</param>
        /// <returns>True if the operation succeds, false otherwise</returns>
        public static async Task<bool> WriteAsync(string filePath, string fileContent)
        {
            return await WriteAsync(ApplicationData.Current.LocalFolder, filePath, fileContent);
        }

        /// <summary>
        /// Async method to write a file in the given StorageFolder
        /// </summary>
        /// <param name="storageFolder">StorageFolder where to look for the file</param>
        /// <param name="filePath">Relative path to a file in the given StorageFolder</param>
        /// <param name="fileContent">File text content to write</param>
        /// <returns>True if the operation succeds, false otherwise</returns>
        public static async Task<bool> WriteAsync(StorageFolder storageFolder, string filePath, string fileContent)
        {
            bool ret = false;
            if (filePath == null) {
                return ret;
            }


            // validate argument
            if (storageFolder == null) {
                throw new ArgumentNullException(nameof(storageFolder));
            }

            try {
                await LockFileAsync(filePath);
                var file = await storageFolder.CreateFileAsync(filePath,
                                CreationCollisionOption.ReplaceExisting);
                // Write text
                await FileIO.WriteTextAsync(file, fileContent);
                ret = true;
            } catch (FileNotFoundException) {
                // File does not exists
                Debug.WriteLine($"WriteAsync - [FileNotFoundException] path : {filePath}");
            } catch (UnauthorizedAccessException) {
                // You don't have permission to access the specified file.
            } catch (Exception e) {
                Debug.WriteLine("[Write] " + e.Message);
                throw;
            } finally {
                // release file's semaphore
                await UnlockFileAsync(filePath);
            }

            return ret;
        }

        /// <summary>
        /// Async method to remove a file from UWP app's local folder
        /// </summary>
        /// <param name="filePath">Relative path to a file in UWP app's local folder</param>
        /// <returns>True if the operation succeds, false otherwise</returns>
        public static async Task<string> RemoveAsync(string filePath)
        {
            return await RemoveAsync(ApplicationData.Current.LocalFolder, filePath);
        }
        /// <summary>
        /// Async method to remove a file from a given StorageFolder
        /// </summary>
        /// <param name="storageFolder">StorageFolder where to look for the file</param>
        /// <param name="filePath">Relative path to a file in UWP app's local folder</param>
        /// <returns>An error message, empty if the operation completed successfully</returns>
        public static async Task<string> RemoveAsync(StorageFolder storageFolder, string filePath)
        {
            string errorMessage = "";

            // validate argument
            if (storageFolder == null) {
                throw new ArgumentNullException(nameof(storageFolder));
            }

            try {
                await LockFileAsync(filePath);
                var file = await storageFolder.GetFileAsync(filePath);
                // Remove file
                await file.DeleteAsync();
            } catch (FileNotFoundException) {
                errorMessage = "FileNotFound";
            } catch (UnauthorizedAccessException) {
                errorMessage = "UnauthorizedAccessException";
            } catch (Exception e) {
                Debug.WriteLine("[Remove] " + e.Message);
                throw;
            } finally {
                // release file's semaphore
                await UnlockFileAsync(filePath);
            }
            return errorMessage;
        }

        /// <summary>
        /// Async method to lock a file
        /// </summary>
        /// <param name="filePath">Relative path to a file in UWP app's local folder</param>
        private static async Task LockFileAsync(string filePath)
        {
            SemaphoreSlim semaphore;
            {
                try {
                    await _dictionarySemaphore.WaitAsync();
                    if (!_fileSystemSemaphores.ContainsKey(filePath)) {
                        // initialize a new semaphore with 1 available request and that accepts 1 concurrent request
                        _fileSystemSemaphores[filePath] = new SemaphoreSlim(MAX_CONCURRENT_REQUEST, MAX_CONCURRENT_REQUEST);
                    }
                    semaphore = _fileSystemSemaphores[filePath];
                } finally {
                    _dictionarySemaphore.Release();
                }
            }
            await semaphore.WaitAsync();
        }

        /// <summary>
        /// Async method to unlock a file
        /// </summary>
        /// <param name="filePath">Relative path to a file in UWP app's local folder</param>
        private static async Task UnlockFileAsync(string filePath)
        {
            // get file's semaphore from the dictionary
            SemaphoreSlim semaphore = null;
            try {
                await _dictionarySemaphore.WaitAsync();
                if (_fileSystemSemaphores.ContainsKey(filePath)) {
                    semaphore = _fileSystemSemaphores[filePath];
                }
            } finally {
                _dictionarySemaphore.Release();
            }

            // release file's semaphore
            if (semaphore != null) {
                semaphore.Release();
                // remove it from semaphores dictionary if not needed anymore => if CurrentCount is the max available count
                try {
                    await _dictionarySemaphore.WaitAsync();
                    if (semaphore.CurrentCount == MAX_CONCURRENT_REQUEST) {
                        semaphore.Dispose();
                        _fileSystemSemaphores.Remove(filePath);
                    }
                } finally {
                    _dictionarySemaphore.Release();
                }
            }
        }
    }
}
