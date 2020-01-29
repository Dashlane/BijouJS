using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Bijou.Chakra;
using Bijou.JSTasks;
using Bijou.NativeFunctions;
using Bijou.Test.UWPChakraHost.Utils;

namespace Bijou.Test.UWPChakraHost.NativeFunctions
{
    [TestClass]
    public class TestJSAsyncFunctions
    {
        private static Mock<UWPChakraHostExecutor> CreateMockExecutor(ICollection<AbstractJSTask> addedCancellableTasks)
        {
            var mock = new Mock<UWPChakraHostExecutor>(MockBehavior.Strict);
            mock.Setup(executor => executor.AddCancellableTask(It.IsAny<AbstractJSTask>()))
                .Callback<AbstractJSTask>(addedCancellableTasks.Add)
                .Returns(123);
            return mock;
        }

        private static JavaScriptValue CreateDummyFunc()
        {
            var funcPropId = JavaScriptPropertyId.FromString("dummy");
            var result = JavaScriptContext.RunScript("function dummy() { return 42; }");
            return JavaScriptValue.GlobalObject.GetProperty(funcPropId);
        }

        private static void TestDelayedJSTask(Func<JavaScriptValue, bool, JavaScriptValue[], ushort, IntPtr, JavaScriptValue> testedFunc, int delay, bool isRepeated, bool withParam)
        {
            using (new UnitTestJsRuntime())
            {
                var addedCancellableTasks = new List<AbstractJSTask>();
                var mockExecutor = CreateMockExecutor(addedCancellableTasks).Object;
                var gcHandleOnMock = GCHandle.Alloc(mockExecutor);
                var dummyFunctionRef = CreateDummyFunc();
                var argsList = new List<JavaScriptValue>();
                argsList.Add(JavaScriptValue.GlobalObject); // The parent object of the called function (from called function point of view, the "this" object)
                argsList.Add(dummyFunctionRef);             // The called function
                var expectedTaskArgsCount = 1;              // The resulting task should only receive its "this" object as parameter
                if (delay != 0)
                {
                    argsList.Add(JavaScriptValue.FromInt32(delay));     // Optional : delayed call
                    if (withParam)
                    {
                        argsList.Add(JavaScriptValue.FromBoolean(true));
                        argsList.Add(JavaScriptValue.FromDouble(3.14));
                        argsList.Add(JavaScriptValue.FromString("test value"));
                        argsList.Add(JavaScriptValue.Null);
                        expectedTaskArgsCount = argsList.Count - 2; // The resulting task should receive all parameters except the called function and the delay
                    }
                }

                var result = testedFunc(JavaScriptValue.Invalid, false, argsList.ToArray(), (ushort)argsList.Count, GCHandle.ToIntPtr(gcHandleOnMock));

                Assert.AreEqual(JavaScriptValueType.Number, result.ValueType);
                Assert.AreEqual(123, result.ToInt32());
                Assert.AreEqual(1, addedCancellableTasks.Count);
                Assert.IsInstanceOfType(addedCancellableTasks[0], typeof(JSTaskFunction));
                var task = addedCancellableTasks[0] as JSTaskFunction;
                Assert.AreEqual(dummyFunctionRef, task.Function);
                Assert.AreEqual(expectedTaskArgsCount, task.Arguments.Length); // Delay 
                Assert.AreEqual(argsList[0], task.Arguments[0]);
                Assert.AreEqual(delay, task.ScheduledDelay);
                Assert.AreEqual(isRepeated, task.ShouldReschedule);
                for (var i = 1; i < expectedTaskArgsCount; i++) // Start at 1 because we already checked index 0 above
                {
                    Assert.AreEqual(argsList[i + 2], task.Arguments[i]); // +2 offset because called function and delay are not passed
                }
            }
        }

        [DataTestMethod]
        [DataRow(0, false)]     // No delay
        [DataRow(1, false)]     // 1ms delay
        [DataRow(10000, false)] // 10s delay
        [DataRow(100, true)]    // 100ms delay and optional call parameters
        public void Test_SetTimeout_ValidCall(int delay, bool withParam)
        {
            TestDelayedJSTask(JSAsyncFunctions.SetTimeoutJavaScriptNativeFunction, delay, false, withParam);
        }

        [TestMethod]
        public void Test_SetTimeout_BadCallbackData()
        {
            using (new UnitTestJsRuntime())
            {
                var badCallbackData = (IntPtr) 0;
                var args = new [] { JavaScriptValue.GlobalObject, CreateDummyFunc() };

                var result = JSAsyncFunctions.SetTimeoutJavaScriptNativeFunction(JavaScriptValue.Invalid, false, args, (ushort) args.Length, badCallbackData);

                Assert.AreEqual(JavaScriptValue.Invalid, result);
            }
        }

        [TestMethod]
        public void Test_SetTimeout_MissingCalledFunction()
        {
            using (new UnitTestJsRuntime())
            {
                var addedCancellableTasks = new List<AbstractJSTask>();
                var mockExecutor = CreateMockExecutor(addedCancellableTasks).Object;
                var gcHandleOnMock = GCHandle.Alloc(mockExecutor);
                var args = new []{ JavaScriptValue.GlobalObject };

                var result = JSAsyncFunctions.SetTimeoutJavaScriptNativeFunction(JavaScriptValue.Invalid, false, args, (ushort)args.Length, GCHandle.ToIntPtr(gcHandleOnMock));

                Assert.AreEqual(JavaScriptValue.Invalid, result);
            }
        }

        [TestMethod]
        public void Test_SetTimeout_WrongDelayParamType()
        {
            using (new UnitTestJsRuntime())
            {
                var addedCancellableTasks = new List<AbstractJSTask>();
                var mockExecutor = CreateMockExecutor(addedCancellableTasks).Object;
                var gcHandleOnMock = GCHandle.Alloc(mockExecutor);
                var args = new []{ JavaScriptValue.GlobalObject, CreateDummyFunc(), JavaScriptValue.FromString("invalid delay") };

                var result = JSAsyncFunctions.SetTimeoutJavaScriptNativeFunction(JavaScriptValue.Invalid, false, args, (ushort)args.Length, GCHandle.ToIntPtr(gcHandleOnMock));

                Assert.AreEqual(JavaScriptValue.Invalid, result);
            }
        }

        [DataTestMethod]
        [DataRow(10, false)]    // 10ms interval
        [DataRow(10000, false)] // 10s interval
        [DataRow(100, true)]    // 100ms delay and optional call parameters
        public void Test_SetInterval_ValidCall(int delay, bool withParam)
        {
            TestDelayedJSTask(JSAsyncFunctions.SetIntervalJavaScriptNativeFunction, delay, true, withParam);
        }

        [TestMethod]
        public void Test_SetInterval_BadCallbackData()
        {
            using (new UnitTestJsRuntime())
            {
                var badCallbackData = (IntPtr) 0;
                var args = new []{ JavaScriptValue.GlobalObject, CreateDummyFunc(), JavaScriptValue.FromInt32(100) };

                var result = JSAsyncFunctions.SetIntervalJavaScriptNativeFunction(JavaScriptValue.Invalid, false, args, (ushort)args.Length, badCallbackData);

                Assert.AreEqual(JavaScriptValue.Invalid, result);
            }
        }

        [TestMethod]
        public void Test_SetInterval_MissingCalledFunction()
        {
            using (new UnitTestJsRuntime())
            {
                var addedCancellableTasks = new List<AbstractJSTask>();
                var mockExecutor = CreateMockExecutor(addedCancellableTasks).Object;
                var gcHandleOnMock = GCHandle.Alloc(mockExecutor);
                var args = new []{ JavaScriptValue.GlobalObject };

                var result = JSAsyncFunctions.SetIntervalJavaScriptNativeFunction(JavaScriptValue.Invalid, false, args, (ushort)args.Length, GCHandle.ToIntPtr(gcHandleOnMock));

                Assert.AreEqual(JavaScriptValue.Invalid, result);
            }
        }

        [TestMethod]
        public void Test_SetInterval_MissingDelay()
        {
            using (new UnitTestJsRuntime())
            {
                var addedCancellableTasks = new List<AbstractJSTask>();
                var mockExecutor = CreateMockExecutor(addedCancellableTasks).Object;
                var gcHandleOnMock = GCHandle.Alloc(mockExecutor);
                var args = new []{ JavaScriptValue.GlobalObject, CreateDummyFunc() };

                var result = JSAsyncFunctions.SetIntervalJavaScriptNativeFunction(JavaScriptValue.Invalid, false, args, (ushort)args.Length, GCHandle.ToIntPtr(gcHandleOnMock));

                Assert.AreEqual(JavaScriptValue.Invalid, result);
            }
        }

        [TestMethod]
        public void Test_SetInterval_WrongDelayParamType()
        {
            using (new UnitTestJsRuntime())
            {
                var addedCancellableTasks = new List<AbstractJSTask>();
                var mockExecutor = CreateMockExecutor(addedCancellableTasks).Object;
                var gcHandleOnMock = GCHandle.Alloc(mockExecutor);
                var args = new []{ JavaScriptValue.GlobalObject, CreateDummyFunc(), JavaScriptValue.FromString("invalid delay") };

                var result = JSAsyncFunctions.SetIntervalJavaScriptNativeFunction(JavaScriptValue.Invalid, false, args, (ushort)args.Length, GCHandle.ToIntPtr(gcHandleOnMock));

                Assert.AreEqual(JavaScriptValue.Invalid, result);
            }
        }

        private Mock<UWPChakraHostExecutor> CreateMockExecutorForClearScheduled(Action<int> action)
        {
            var mock = new Mock<UWPChakraHostExecutor>(MockBehavior.Strict);
            mock.Setup(executor => executor.CancelTask(It.IsAny<int>()))
                .Callback(action);
            return mock;
        }

        [TestMethod]
        public void Test_ClearScheduled_ValidCall()
        {
            using (new UnitTestJsRuntime())
            {
                var canceledId = 0;
                var cancelCount = 0;
                var mockExecutor = CreateMockExecutorForClearScheduled(i => { canceledId = i; cancelCount++; }).Object;
                var gcHandleOnMock = GCHandle.Alloc(mockExecutor);
                var args = new []{ JavaScriptValue.GlobalObject, JavaScriptValue.FromInt32(123) };

                var result = JSAsyncFunctions.ClearScheduledJavaScriptNativeFunction(JavaScriptValue.Invalid, false, args, (ushort)args.Length, GCHandle.ToIntPtr(gcHandleOnMock));

                Assert.AreEqual(JavaScriptValue.Invalid, result);
                Assert.AreEqual(123, canceledId);
                Assert.AreEqual(1, cancelCount);
            }
        }

        [TestMethod]
        public void Test_ClearScheduled_BadCallbackData()
        {
            using (new UnitTestJsRuntime())
            {
                var badCallbackData = (IntPtr) 0;
                var args = new []{ JavaScriptValue.GlobalObject, JavaScriptValue.FromInt32(123) };

                var result = JSAsyncFunctions.ClearScheduledJavaScriptNativeFunction(JavaScriptValue.Invalid, false, args, (ushort)args.Length, badCallbackData);

                Assert.AreEqual(JavaScriptValue.Invalid, result);
            }
        }

        [TestMethod]
        public void Test_ClearScheduled_MissingTaskId()
        {
            using (new UnitTestJsRuntime())
            {
                var cancelCount = 0;
                var mockExecutor = CreateMockExecutorForClearScheduled(i => cancelCount++).Object;
                var gcHandleOnMock = GCHandle.Alloc(mockExecutor);
                var args = new []{ JavaScriptValue.GlobalObject };

                var result = JSAsyncFunctions.ClearScheduledJavaScriptNativeFunction(JavaScriptValue.Invalid, false, args, (ushort)args.Length, GCHandle.ToIntPtr(gcHandleOnMock));

                Assert.AreEqual(JavaScriptValue.Invalid, result);
                Assert.AreEqual(0, cancelCount);
            }
        }

        [TestMethod]
        public void Test_ClearScheduled_WrongTaskIdParamType()
        {
            using (new UnitTestJsRuntime())
            {
                var cancelCount = 0;
                var mockExecutor = CreateMockExecutorForClearScheduled(i => cancelCount++).Object;
                var gcHandleOnMock = GCHandle.Alloc(mockExecutor);
                var args = new []{ JavaScriptValue.GlobalObject, JavaScriptValue.FromString("this should be a number") };

                var result = JSAsyncFunctions.ClearScheduledJavaScriptNativeFunction(JavaScriptValue.Invalid, false, args, (ushort)args.Length, GCHandle.ToIntPtr(gcHandleOnMock));

                Assert.AreEqual(JavaScriptValue.Invalid, result);
                Assert.AreEqual(0, cancelCount);
            }
        }

    }
}