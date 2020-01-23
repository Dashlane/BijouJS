// enable script serialization, serialized script should be added as a reference to the caller as long as the script is needed
//#define USE_SCRIPT_SERIALIZATION

using Bijou.Chakra.Hosting;

namespace Bijou.JSTasks
{
    internal sealed class JSTaskScript : AbstractJSTask
    {
        private readonly string _scriptPath;
        private readonly string _script;
        private JavaScriptSourceContext _currentSourceContext;

#if USE_SCRIPT_SERIALIZATION && !DEBUG
        private byte[] _serializedScript =  null;
#endif

        public JSTaskScript(string scriptPath, string script, JavaScriptSourceContext currentSourceContext)
        {
            _scriptPath = scriptPath;
            _script = script;
            _currentSourceContext = currentSourceContext;
        }

        protected override JavaScriptValue ExecuteImpl()
        {
            if (IsCanceled) 
            {
                return JavaScriptValue.Invalid;
            }

#if USE_SCRIPT_SERIALIZATION && !DEBUG
            const ulong DEFAULT_BUFFER_SIZE = 10 * 1024 * 1024;

            // serialize script to improve performance
            if (_serializedScript == null) {
                // first, use a large buffer (10 MB) to define needed buffer size
                // using a null buffer triggers an exception
                _serializedScript = new byte[DEFAULT_BUFFER_SIZE];
                var bufferSize = JavaScriptContext.SerializeScript(_script, _serializedScript);
                // resize to minimum needed size
                if (bufferSize != DEFAULT_BUFFER_SIZE) {
                    _serializedScript = new byte[bufferSize];
                    JavaScriptContext.SerializeScript(_script, _serializedScript);
                }
            }

            //
            // Run the script.
            //
            return JavaScriptContext.RunScript(_script, _serializedScript, _currentSourceContext++, _scriptPath);
#else
            //
            // Run the script.
            //
            return JavaScriptContext.RunScript(_script, _currentSourceContext++, _scriptPath);
#endif

        }
    }
}
