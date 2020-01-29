using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Bijou.NativeFunctions;
using Bijou.Test.UWPChakraHost.Utils;

namespace Bijou.Test.UWPChakraHost.NativeFunctions
{
    [TestClass]
    public class TestJSSendToHost
    {
        private Mock<UWPChakraHostExecutor> CreateMockExecutor(ICollection<string> receivedMessages)
        {
            var mock = new Mock<UWPChakraHostExecutor>(MockBehavior.Strict);
            mock.Setup(executor => executor.OnMessageReceived(It.IsAny<string>()))
                .Callback<string>(receivedMessages.Add);
            return mock;
        }
        
        [DataTestMethod]
        [DataRow("TestMessage")]
        [DataRow("")]
        public void Test_PassInputStringToOnMessageReceived(string message)
        {
            using (new UnitTestJsRuntime())
            {
                var receivedMessages = new List<string>();
                var mockExecutor = CreateMockExecutor(receivedMessages).Object;
                var gcHandleOnMock = GCHandle.Alloc(mockExecutor);
                var args = new []{ JavaScriptValue.Invalid, JavaScriptValue.FromString(message) };

                var result = JSSendToHost.SendToHostJavaScriptNativeFunction(JavaScriptValue.Invalid, false, args, (ushort)args.Length, GCHandle.ToIntPtr(gcHandleOnMock));

                Assert.AreEqual(JavaScriptValue.Invalid, result);
                Assert.AreEqual(1, receivedMessages.Count);
                Assert.AreEqual(message, receivedMessages[0]);
            }
        }

        [TestMethod]
        public void Test_ReturnInvalidOnNullCallbackData()
        {
            using (new UnitTestJsRuntime())
            {
                var badCallbackData = (IntPtr) 0;
                var args = new []{ JavaScriptValue.Invalid, JavaScriptValue.FromString("TestMessage") };

                var result = JSSendToHost.SendToHostJavaScriptNativeFunction(JavaScriptValue.Invalid, false, args, (ushort)args.Length, badCallbackData);

                Assert.AreEqual(JavaScriptValue.Invalid, result);
            }
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(3)]
        [DataRow(10)]
        public void Test_NoActionOnInvalidArgCount(int argCount)
        {
            using (new UnitTestJsRuntime())
            {
                var receivedMessages = new List<string>();
                var mockExecutor = CreateMockExecutor(receivedMessages).Object;
                var gcHandleOnMock = GCHandle.Alloc(mockExecutor);
                var args = new JavaScriptValue[argCount];
                for (var i = 0; i < argCount; i++)
                {
                    args[i] = (i == 0 ? JavaScriptValue.Invalid : JavaScriptValue.FromString("TestMessage"));
                }

                var result = JSSendToHost.SendToHostJavaScriptNativeFunction(JavaScriptValue.Invalid, false, args, (ushort)args.Length, GCHandle.ToIntPtr(gcHandleOnMock));

                Assert.AreEqual(JavaScriptValue.Invalid, result);
                Assert.AreEqual(0, receivedMessages.Count);
            }
        }

        [TestMethod]
        public void Test_NoActionOnInvalidArg()
        {
            using (new UnitTestJsRuntime())
            {
                var receivedMessages = new List<string>();
                var mockExecutor = CreateMockExecutor(receivedMessages).Object;
                var gcHandleOnMock = GCHandle.Alloc(mockExecutor);
                var args = new []{ JavaScriptValue.Invalid, JavaScriptValue.Invalid };

                var result = JSSendToHost.SendToHostJavaScriptNativeFunction(JavaScriptValue.Invalid, false, args, (ushort)args.Length, GCHandle.ToIntPtr(gcHandleOnMock));

                Assert.AreEqual(JavaScriptValue.Invalid, result);
                Assert.AreEqual(0, receivedMessages.Count);
            }
        }

        [TestMethod]
        public void Test_NoActionOnWrongArgType()
        {
            using (new UnitTestJsRuntime())
            {
                var receivedMessages = new List<string>();
                var mockExecutor = CreateMockExecutor(receivedMessages).Object;
                var gcHandleOnMock = GCHandle.Alloc(mockExecutor);
                var args = new []{ JavaScriptValue.Invalid, JavaScriptValue.FromInt32(42) };

                var result = JSSendToHost.SendToHostJavaScriptNativeFunction(JavaScriptValue.Invalid, false, args, (ushort)args.Length, GCHandle.ToIntPtr(gcHandleOnMock));

                Assert.AreEqual(JavaScriptValue.Invalid, result);
                Assert.AreEqual(0, receivedMessages.Count);
            }
        }
    }
}
