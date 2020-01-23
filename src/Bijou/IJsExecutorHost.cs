using System;

namespace Bijou
{
    public interface IJsExecutorHost : IDisposable
    {
        event Action<string> MessageReady;
        event Action<string> JsExecutionFailed;

        /// <summary>
        ///     Load a script and run it adding it to the event loop
        /// </summary>
        void LoadAndRunScriptAsync(string scriptPath);

        /// <summary>
        ///     Run a script adding it to the event loop
        /// </summary>
        void RunScriptAsync(string script);

        /// <summary>
        ///     Run a script adding it to the event loop
        ///     
        /// </summary>
        void RunScriptAsync(string script, string scriptPath);

        void CallFunctionAsync(string function, params object[] arguments);
    }
}
