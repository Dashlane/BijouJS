using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Bijou.Chakra;
using Bijou.Test.UWPChakraHost.Utils;

namespace Bijou.Test.UWPChakraHost.JSTasks
{
    [TestClass]
    public class TestJSTaskAbstract
    {
        private class JsTaskExecute : AbstractJSTask
        {
            public JsTaskExecute(int delay = 0, bool shouldReschedule = false) :
                base(delay, shouldReschedule) { }

            protected override JavaScriptValue ExecuteImpl()
            {
                return JavaScriptValue.True;
            }
        }

        private Mock<AbstractJSTask> CreateMock(int delay = 0, bool shouldReschedule = false)
        {
            return new Mock<AbstractJSTask>(MockBehavior.Default, delay, shouldReschedule) { CallBase = true };
        }

        [TestMethod]
        public void TestScheduledTime()
        {
            using (new UnitTestJsRuntime())
            {
                var jsTask = CreateMock();
                Assert.AreEqual(0, jsTask.Object.ScheduledDelay);
                Assert.AreEqual(0, jsTask.Object.MillisecondsToExecution);
                Assert.IsTrue(jsTask.Object.ScheduledTime <= DateTime.UtcNow);
                Assert.IsTrue(jsTask.Object.IsReadyForExecution);

                // create a task with a delay of 20 seconds
                jsTask = CreateMock(20000);
                Assert.AreEqual(20000, jsTask.Object.ScheduledDelay);
                // The execution should be in the future
                Assert.IsTrue(jsTask.Object.MillisecondsToExecution > 0);
                Assert.IsTrue(jsTask.Object.ScheduledTime > DateTime.UtcNow);

                var previousScheduleTime = jsTask.Object.ScheduledTime;
                TestUtils.Wait(300);
                jsTask.Object.ResetScheduledTime();
                Assert.IsTrue(jsTask.Object.ScheduledTime > previousScheduleTime);
            }
        }

        [TestMethod]
        public void TestCancel()
        {
            using (new UnitTestJsRuntime())
            {
                var jsTask = CreateMock(0, true);
                Assert.IsFalse(jsTask.Object.IsCanceled);
                Assert.IsTrue(jsTask.Object.ShouldReschedule);
                jsTask.Object.Cancel();
                Assert.IsTrue(jsTask.Object.IsCanceled);
                Assert.IsFalse(jsTask.Object.ShouldReschedule);
            }
        }

        [TestMethod]
        public void TestExecute()
        {
            using (new UnitTestJsRuntime())
            {
                var testTask = new JsTaskExecute();
                Assert.AreEqual(JavaScriptValue.True, testTask.Execute());

                testTask = new JsTaskExecute(30000);
                testTask.Cancel();
                Assert.AreNotEqual(JavaScriptValue.True, testTask.Execute());
            }
        }
    }
}