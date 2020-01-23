using Bijou.Chakra.Hosting;
using System;
using System.Collections.Generic;

namespace Bijou.JSTasks
{
    internal class JSTaskFunction : AbstractJSTask
    {
        private bool _isReleased;

        // These members store parameters passed as native types, to be converted to JS values at task execution time (when we know the JavaScriptContext is available)
        private readonly string _functionNativeName;
        private readonly object[] _nativeArguments;

        public JavaScriptValue Function { get; private set; }

        public JavaScriptValue[] Arguments { get; private set; }

        // This must be called when the JavaScriptContext is available (i.e. in the thread which owns it)
        // The first element of arguments must be a reference to the parent object ("this") of the function. Use JavaScriptValue.GlobalObject when calling a global function
        // Note: If function is JavaScriptValue.Invalid, an error is logged at construction and Execute() returns JavaScriptValue.Invalid,
        //       but if any element of arguments is JavaScriptValue.Invalid, no error is logged and Execute() raises JavaScriptUsageException.
        public JSTaskFunction(JavaScriptValue function, JavaScriptValue[] arguments, int delay = 0, bool repeat = false)
            : base(delay, repeat)
        {
            ValidateAndStoreValues(function, arguments);
        }

        // Convenience constructor to call a global JS function using C# native values instead of references to JS values
        // You don't need to specify the parent object ("this") it will use the global object
        // This can be called from any thread
        public JSTaskFunction(string functionName, params object[] arguments)
        {
            _functionNativeName = functionName;
            _nativeArguments = arguments;
        }

        ~JSTaskFunction()
        {
            if (!_isReleased) 
            {
                Console.Error.WriteLine("~JSTaskFunction: Memory leak in JS context");
            }
        }

        // This must be called when the JavaScriptContext is available (i.e. in the thread which owns it)
        private void ProjectNativeParameters()
        {
            if (string.IsNullOrEmpty(_functionNativeName))
            {
                return;
            }

            var func = JavaScriptValue.GlobalObject.GetProperty(JavaScriptPropertyId.FromString(_functionNativeName));
            var args = new List<JavaScriptValue> { JavaScriptValue.GlobalObject };
            foreach (var parameter in _nativeArguments) 
            {
                switch (parameter.GetType().Name)
                {
                    case "Int32":
                        args.Add(JavaScriptValue.FromInt32((int)parameter));
                        break;
                    case "Double":
                        args.Add(JavaScriptValue.FromDouble((double)parameter));
                        break;
                    case "Boolean":
                        args.Add(JavaScriptValue.FromBoolean((bool)parameter));
                        break;
                    case "String":
                        args.Add(JavaScriptValue.FromString((string)parameter));
                        break;
                    default:
                        throw new Exception("Not supported type: " + parameter.GetType().Name);
                }
            }

            ValidateAndStoreValues(func, args.ToArray());
        }

        private void ValidateAndStoreValues(JavaScriptValue function, JavaScriptValue[] arguments)
        {
            // ensure there is a valid context
            if (!JavaScriptContext.IsCurrentValid) 
            {
                Console.Error.WriteLine("JSTaskFunction invalid context");
                return;
            }

            Function = function;
            Arguments = arguments;

            if (!Function.IsValid) 
            {
                Console.Error.WriteLine("JSTaskFunction invalid function");
                return;
            }

            // keep reference since this is unmanaged memory
            Function.AddRef();
            foreach (var arg in Arguments)
            {
                if (arg.IsValid)
                {
                    arg.AddRef();
                }
            }
        }

        protected override JavaScriptValue ExecuteImpl()
        {
            var ret = JavaScriptValue.Invalid;
            if (!Function.IsValid) 
            {
                ProjectNativeParameters();
            }

            if (Function.IsValid) 
            {
                ret = Function.CallFunction(Arguments);
            }

            return ret;
        }

        protected override void ReleaseJsResources()
        {
            // ensure there is a valid context
            if (!JavaScriptContext.IsCurrentValid) 
            {
                Console.Error.WriteLine("ReleaseJsResources invalid context");
                return;
            }

            if (ShouldReschedule || _isReleased) 
            {
                return;
            }

            if (Function.IsValid) 
            {
                Function.Release();
            }

            foreach (var arg in Arguments) 
            {
                if (arg.IsValid) 
                {
                    arg.Release();
                }
            }

            _isReleased = true;
        }
    }
}
