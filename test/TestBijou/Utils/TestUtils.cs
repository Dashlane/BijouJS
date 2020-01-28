using System;
using System.Threading.Tasks;

namespace Bijou.Test.UWPChakraHost.Utils
{
    public static class TestUtils
    {
        public static void Wait(int milliseconds)
        {
            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();
        }
    }
}
