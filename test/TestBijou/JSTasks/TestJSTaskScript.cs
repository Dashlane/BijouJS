using Bijou.Chakra;
using Bijou.JSTasks;
using Bijou.Test.UWPChakraHost.Utils;
using FluentResults;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bijou.Test.UWPChakraHost.JSTasks
{
    [TestClass]
    public class TestJSTaskScript
    {
        private Result<JavaScriptValue> RunScript(string script)
        {
            var jsTask = new JSTaskScript(string.Empty, script, JavaScriptSourceContext.None);
            return jsTask.Execute();
        }

        private void VerifyScript(string script, JavaScriptValueType jsTypeExpected, int valueExpected)
        {
            var result = RunScript(script);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(jsTypeExpected, result.Value.ValueType.Value);
            Assert.AreEqual(valueExpected, result.Value.ToInt32().Value);
        }

        private void VerifyScript(string script, JavaScriptValueType jsTypeExpected, double valueExpected)
        {
            var result = RunScript(script);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(jsTypeExpected, result.Value.ValueType.Value);
            Assert.AreEqual(valueExpected, result.Value.ToDouble().Value, 0.00001);
        }

        private void VerifyScript(string script, JavaScriptValueType jsTypeExpected, bool valueExpected)
        {
            var result = RunScript(script);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(jsTypeExpected, result.Value.ValueType.Value);
            Assert.AreEqual(valueExpected, result.Value.ToBoolean().Value);
        }

        private void VerifyScript(string script, JavaScriptValueType jsTypeExpected, string valueExpected)
        {
            var result = RunScript(script);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(jsTypeExpected, result.Value.ValueType.Value);
            Assert.AreEqual(valueExpected, result.Value.AsString());
        }

        [TestMethod]
        public void Test_Script()
        {
            using (new UnitTestJsRuntime())
            {
                var script = @"
                function add (x, y) { return x + y;};
                add(2,3);
                ";
                VerifyScript(script, JavaScriptValueType.Number, 5);

                script = @"
                'coucou';
                ";
                VerifyScript(script, JavaScriptValueType.String, "coucou");

                script = @"
                true;
                ";
                VerifyScript(script, JavaScriptValueType.Boolean, true);

                script = @"
                2.5*6.3;
                ";
                VerifyScript(script, JavaScriptValueType.Number, 15.75);
            }
        }
    }
}