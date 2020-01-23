namespace Bijou
{
    public interface IJsExecutorFactory
    {
        /// <summary>
        /// Provide a new instance of an object implementing IJsExecutorHost
        /// Note that IJsExecutorHost objects are also IDisposable so the caller must also Dispose() it when it is no longer needed
        /// The implementation may decide to use a single JS engine instance for all IJsExecutorHost objects, as long as they are isolated
        /// (code running in one should not impact code running in the others)
        /// </summary>
        /// <returns>The newly created IJsExecutorHost object</returns>
        IJsExecutorHost CreateJsExecutorHost();
    }
}
