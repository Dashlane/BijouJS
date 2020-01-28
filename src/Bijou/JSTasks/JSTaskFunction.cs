using System;
using System.Collections.Generic;
using System.Linq;
using Bijou.Chakra;
using Bijou.Types;
using FluentResults;

namespace Bijou.JSTasks
{
    internal class JSTaskFunction : AbstractJSTask
    {
        private bool _isReleased;

        // These members store parameters passed as native types, to be converted to JS values at task execution time (when we know the JavaScriptContext is available)
        private readonly string _functionNativeName;
        private readonly object[] _nativeArguments;

        public JavaScriptFunction Function { get; private set; }

        public JavaScriptObject[] Arguments { get; private set; }

        // This must be called when the JavaScriptContext is available (i.e. in the thread which owns it)
        // The first element of arguments must be a reference to the parent object ("this") of the function. Use JavaScriptValue.GlobalObject when calling a global function
        // Note: If function is JavaScriptValue.Invalid, an error is logged at construction and Execute() returns JavaScriptValue.Invalid,
        //       but if any element of arguments is JavaScriptValue.Invalid, no error is logged and Execute() raises JavaScriptUsageException.
        public JSTaskFunction(
            JavaScriptFunction function, 
            JavaScriptObject[] arguments,
            int delay = 0, 
            bool repeat = false)
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
        private Result ProjectNativeParameters()
        {
            if (string.IsNullOrEmpty(_functionNativeName))
            {
                return Results.Fail("No native function name provided");
            }

            var result = JavaScriptPrototype.GlobalObject;
            if (result.IsFailed)
            {
                return Results.Fail("Failed to get Global Object");
            }

            var propertyId = JavaScriptPropertyId.FromString(_functionNativeName);
            if (propertyId.IsFailed)
            {
                return Results.Fail($"Failed to find property called {_functionNativeName}");
            }

            var globalObject = result.Value;
            var function = globalObject.GetProperty<JavaScriptFunction>(_functionNativeName);
            if (function.IsFailed)
            {
                return Results.Fail($"Failed to find method {_functionNativeName} on Global Object");
            }

            var func = function.Value;
            var args = new List<JavaScriptObject> { globalObject };
            foreach (var parameter in _nativeArguments)
            {
                var argument = Results.Fail($"Not supported type: {parameter.GetType().Name}");
                switch (parameter.GetType().Name)
                {
                    case "Int32":
                        argument = JavaScriptNumber.FromInt32((int)parameter);
                        break;

                    case "Double":
                        argument = JavaScriptNumber.FromDouble((double)parameter);
                        break;

                    case "Boolean":
                        argument = JavaScriptBoolean.FromBoolean((bool)parameter);
                        break;

                    case "String":
                        argument = JavaScriptString.FromString((string)parameter);
                        break;
                }

                if (argument.IsFailed)
                {
                    return Results.Fail(argument.Errors.First());
                }

                args.Add(argument.ToResult<JavaScriptObject>().Value);
            }

            return ValidateAndStoreValues(func, args.ToArray());
        }

        private Result ValidateAndStoreValues(JavaScriptFunction function, JavaScriptObject[] arguments)
        {
            if (!JavaScriptContext.IsCurrentValid) 
            {
                return Results.Fail("JSTaskFunction invalid context");
            }

            Function = function;
            Arguments = arguments;

            if (!Function.IsValid) 
            {
                return Results.Fail("JSTaskFunction invalid function");
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

            return Results.Ok();
        }

        protected override Result<JavaScriptObject> ExecuteImpl()
        {
            if (!Function.IsValid)
            {
                var result = ProjectNativeParameters();
                if (result.IsFailed)
                {
                    return Results.Fail(result.Errors.First());
                }
            }

            return Function.CallFunction<JavaScriptObject>(Arguments); ;
        }

        protected override void ReleaseJsResources()
        {
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
