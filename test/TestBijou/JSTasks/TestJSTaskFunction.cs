using Bijou.Chakra;
using Bijou.JSTasks;
using Bijou.Test.UWPChakraHost.Utils;
using FluentResults;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bijou.Test.UWPChakraHost.JSTasks
{
    [TestClass]
    public class TestJSTaskFunction
    {
        private const string Functions = @"
            function funcNoParam() { return 42; }
            function funcIdentity(x) { return x; }
            function funcThreeParam(x, y, z) { return (x * y) / z; }
            function concat(a, b) { return a + b;}
            function isOdd(n) { return n % 2 === 0;}
            function square(x) { return x * x;}
            function fib(n) { return n == 0 ? 0 : n + fib(n-1); }
        ";

        private Result<JavaScriptValue> InjectFunctionsInContext()
        {
            var jsTask = new JSTaskScript(string.Empty, Functions, JavaScriptSourceContext.None);
            return jsTask.Execute();
        }

        private Result<JavaScriptValue> GetGlobalJsValueByName(string name)
        {
            return JavaScriptValue.GlobalObject.Value.GetProperty(name);
        }

        [DataTestMethod]
        [DataRow("funcNoParam", 0, "", "", "", 0, false, "42")]
        [DataRow("funcIdentity", 1, "100", "", "", 20, true, "100")]
        [DataRow("concat", 2, "Hello ", "world", "", 1000, false, "Hello world")]
        [DataRow("funcThreeParam", 3, "3", "5", "2", 0, false, "7.5")]
        public void TestExecuteValidCall(string funcName, int argCount, string arg1, string arg2, string arg3, int delay, bool repeat, string expectedResult)
        {
            using (new UnitTestJsRuntime())
            {
                var args = new JavaScriptValue[argCount + 1];
                args[0] = JavaScriptValue.GlobalObject.Value;
                InjectFunctionsInContext();
                if (argCount >= 1) args[1] = JavaScriptValue.FromString(arg1).Value;
                if (argCount >= 2) args[2] = JavaScriptValue.FromString(arg2).Value;
                if (argCount >= 3) args[3] = JavaScriptValue.FromString(arg3).Value;
                var task = new JSTaskFunction(GetGlobalJsValueByName(funcName).Value, args, delay, repeat);

                var result = task.Execute();

                Assert.AreEqual(expectedResult, result.Value.ConvertToString().Value.AsString()); 
                Assert.AreEqual(delay, task.ScheduledDelay);
                Assert.AreEqual(repeat, task.ShouldReschedule);
            }
        }

        [TestMethod]
        public void TestExecuteUsingCSharpArguments()
        {
            using (new UnitTestJsRuntime())
            {
                var result = InjectFunctionsInContext();
                Assert.IsTrue(result.IsSuccess);

                var jsTask = new JSTaskFunction("concat", "Hello, ", "This is Patrick.");
                result = jsTask.Execute();
                Assert.AreEqual(JavaScriptValueType.String, result.Value.ValueType.Value);
                Assert.AreEqual("Hello, This is Patrick.", result.Value.AsString());

                jsTask = new JSTaskFunction("isOdd", 5);
                result = jsTask.Execute();
                Assert.AreEqual(JavaScriptValueType.Boolean, result.Value.ValueType.Value);
                Assert.AreEqual(false, result.Value.ToBoolean().Value);

                jsTask = new JSTaskFunction("isOdd", 24);
                result = jsTask.Execute();
                Assert.AreEqual(JavaScriptValueType.Boolean, result.Value.ValueType.Value);
                Assert.AreEqual(true, result.Value.ToBoolean().Value);

                jsTask = new JSTaskFunction("square", 2.5);
                result = jsTask.Execute();
                Assert.AreEqual(JavaScriptValueType.Number, result.Value.ValueType.Value);
                Assert.AreEqual(6.25, result.Value.ToDouble().Value);

                jsTask = new JSTaskFunction("fib", 10);
                result = jsTask.Execute();
                Assert.AreEqual(JavaScriptValueType.Number, result.Value.ValueType.Value);
                Assert.AreEqual(55, result.Value.ToInt32().Value);
            }
        }

        [TestMethod]
        public void TestExecuteInvalidFunctionReturnsInvalid()
        {
            using (new UnitTestJsRuntime())
            {
                var task = new JSTaskFunction(JavaScriptValue.Invalid, 
                    new []
                    {
                        JavaScriptValue.GlobalObject.Value
                    });

                var result = task.Execute();

                Assert.IsTrue(result.IsFailed, "Result should be failed");
            }
        }

        [TestMethod]
        public void TestExecuteInvalidParameter()
        {
            using (new UnitTestJsRuntime())
            {
                InjectFunctionsInContext();
                var func = GetGlobalJsValueByName("funcNoParam");
                var task = new JSTaskFunction(func.Value,
                    new[]
                    {
                        JavaScriptValue.GlobalObject.Value,
                        JavaScriptValue.Invalid
                    });

                var result = task.Execute();

                Assert.IsTrue(result.IsFailed);
            }
        }

        [TestMethod]
        public void TestExecuteMissingParameter()
        {
            using (new UnitTestJsRuntime())
            {
                InjectFunctionsInContext();
                var task = new JSTaskFunction(GetGlobalJsValueByName("funcIdentity").Value,
                    new[]
                    {
                        JavaScriptValue.GlobalObject.Value
                    });

                var result = task.Execute();

                Assert.IsTrue(result.IsSuccess, "Result should be success");
                Assert.AreEqual(result.Value.ValueType.Value, JavaScriptValueType.Undefined, "Result value should be invalid");
            }
        }

        [TestMethod]
        public void TestExecuteExtraParameter()
        {
            using (new UnitTestJsRuntime())
            {
                InjectFunctionsInContext();
                var task = new JSTaskFunction(GetGlobalJsValueByName("funcIdentity").Value,
                    new[]
                    {
                        JavaScriptValue.GlobalObject.Value, JavaScriptValue.FromInt32(10).Value,
                        JavaScriptValue.FromString("unexpected").Value
                    });

                var result = task.Execute();

                // Extra parameters should be ignored, funcIdentity(10) returns 10 so that's what we expect.
                Assert.AreEqual(10, result.Value.ToInt32().Value);
            }
        }

    }
}
