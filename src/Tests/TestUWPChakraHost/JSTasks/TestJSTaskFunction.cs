using Bijou.JSTasks;
using Bijou.Test.UWPChakraHost.Utils;
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
            function isOdd(n) { return n % 2 == 0;}
            function square(x) { return x * x;}
            function fib(n) { return n == 0 ? 0 : n + fib(n-1); }
        ";

        private void InjectFunctionsInContext()
        {
            var jsTask = new JSTaskScript(string.Empty, Functions, JavaScriptSourceContext.None);
            jsTask.Execute();
        }

        private JavaScriptValue GetGlobalJsValueByName(string name)
        {
            return JavaScriptValue.GlobalObject.GetProperty(JavaScriptPropertyId.FromString(name));
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
                InjectFunctionsInContext();
                var args = new JavaScriptValue[argCount + 1];
                args[0] = JavaScriptValue.GlobalObject;
                if (argCount >= 1) args[1] = JavaScriptValue.FromString(arg1);
                if (argCount >= 2) args[2] = JavaScriptValue.FromString(arg2);
                if (argCount >= 3) args[3] = JavaScriptValue.FromString(arg3);
                var task = new JSTaskFunction(GetGlobalJsValueByName(funcName), args, delay, repeat);

                var result = task.Execute();

                Assert.AreEqual(expectedResult, result.ConvertToString().ToString()); 
                Assert.AreEqual(delay, task.ScheduledDelay);
                Assert.AreEqual(repeat, task.ShouldReschedule);
            }
        }

        [TestMethod]
        public void TestExecuteUsingCSharpArguments()
        {
            using (new UnitTestJsRuntime())
            {
                InjectFunctionsInContext();

                var jsTask = new JSTaskFunction("concat", "Hello, ", "This is Patrick.");
                var result = jsTask.Execute();
                Assert.AreEqual(JavaScriptValueType.String, result.ValueType);
                Assert.AreEqual("Hello, This is Patrick.", result.ToString());

                jsTask = new JSTaskFunction("isOdd", 5);
                result = jsTask.Execute();
                Assert.AreEqual(JavaScriptValueType.Boolean, result.ValueType);
                Assert.AreEqual(false, result.ToBoolean());

                jsTask = new JSTaskFunction("isOdd", 24);
                result = jsTask.Execute();
                Assert.AreEqual(JavaScriptValueType.Boolean, result.ValueType);
                Assert.AreEqual(true, result.ToBoolean());

                jsTask = new JSTaskFunction("square", 2.5);
                result = jsTask.Execute();
                Assert.AreEqual(JavaScriptValueType.Number, result.ValueType);
                Assert.AreEqual(6.25, result.ToDouble());

                jsTask = new JSTaskFunction("fib", 10);
                result = jsTask.Execute();
                Assert.AreEqual(JavaScriptValueType.Number, result.ValueType);
                Assert.AreEqual(55, result.ToInt32());
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
                        JavaScriptValue.GlobalObject
                    });

                var result = task.Execute();

                Assert.AreEqual(JavaScriptValue.Invalid, result);
            }
        }

        [TestMethod]
        public void TestExecuteInvalidParameter()
        {
            using (new UnitTestJsRuntime())
            {
                InjectFunctionsInContext();
                var task = new JSTaskFunction(GetGlobalJsValueByName("funcNoParam"),
                    new[]
                    {
                        JavaScriptValue.GlobalObject,
                        JavaScriptValue.Invalid
                    });

                Assert.ThrowsException<JavaScriptUsageException>(() =>
                {
                    task.Execute();
                });
            }
        }

        [TestMethod]
        public void TestExecuteMissingParameter()
        {
            using (new UnitTestJsRuntime())
            {
                InjectFunctionsInContext();
                var task = new JSTaskFunction(GetGlobalJsValueByName("funcIdentity"),
                    new[]
                    {
                        JavaScriptValue.GlobalObject
                    });

                var result = task.Execute();

                Assert.AreEqual(JavaScriptValueType.Undefined, result.ValueType);
            }
        }

        [TestMethod]
        public void TestExecuteExtraParameter()
        {
            using (new UnitTestJsRuntime())
            {
                InjectFunctionsInContext();
                var task = new JSTaskFunction(GetGlobalJsValueByName("funcIdentity"),
                    new[]
                    {
                        JavaScriptValue.GlobalObject, JavaScriptValue.FromInt32(10),
                        JavaScriptValue.FromString("unexpected")
                    });

                var result = task.Execute();

                // Extra parameters should be ignored, funcIdentity(10) returns 10 so that's what we expect.
                Assert.AreEqual(10, result.ToInt32());
            }
        }

    }
}
