using Bijou.Chakra.Hosting;
using System;
using System.Collections.Generic;

namespace Bijou.JSTasks
{
    internal class JSTaskFunction : JSTaskAbstract
    {
        public JavaScriptValue function { get; private set; }
        public JavaScriptValue[] arguments { get; private set; }
        private bool _isReleased = false;

        // These members store parameters passed as native types, to be converted to JS values at task execution time (when we know the JavaScriptContext is available)
        private readonly string functionNativeName;
        private readonly object[] nativeArguments;

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
            functionNativeName = functionName;
            nativeArguments = arguments;
        }

        ~JSTaskFunction()
        {
            if (!_isReleased) {
                Console.Error.WriteLine("~JSTaskFunction: Memory leak in JS context");
            }
        }

        // This must be called when the JavaScriptContext is available (i.e. in the thread which owns it)
        private void ProjectNativeParameters()
        {
            if (string.IsNullOrEmpty(functionNativeName))
            {
                return;
            }

            var func = JavaScriptValue.GlobalObject.GetProperty(JavaScriptPropertyId.FromString(functionNativeName));

            var args = new List<JavaScriptValue> { JavaScriptValue.GlobalObject };
            foreach (var parameter in nativeArguments) {
                switch (parameter.GetType().Name) {
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
            // Ensure there is a valid context.
            if (!JavaScriptContext.IsCurrentValid)
            {
                Console.Error.WriteLine("JSTaskFunction invalid context");
                return;
            }

            this.function = function;
            this.arguments = arguments;

            if (!this.function.IsValid)
            {
                Console.Error.WriteLine("JSTaskFunction invalid function");
                return;
            }

            // Keep reference since this is unmanaged memory.
            this.function.AddRef();
            foreach (var arg in this.arguments) {
                if (arg.IsValid) {
                    arg.AddRef();
                }
            }
        }

        protected override JavaScriptValue ExecuteImpl()
        {
            JavaScriptValue ret = JavaScriptValue.Invalid;

            if (!function.IsValid) {
                ProjectNativeParameters();
            }

            if (function.IsValid) {
                ret = function.CallFunction(arguments);
            }

            return ret;
        }

        protected override void ReleaseJsResources()
        {
            // Ensure there is a valid context.
            if (!JavaScriptContext.IsCurrentValid) {
                Console.Error.WriteLine("ReleaseJsResources invalid context");
                return;
            }

            if (ShouldReschedule || _isReleased) {
                return;
            }

            if (function.IsValid) {
                function.Release();
            }
            foreach (var arg in arguments) {
                if (arg.IsValid) {
                    arg.Release();
                }
            }

            _isReleased = true;
        }
    }
}
