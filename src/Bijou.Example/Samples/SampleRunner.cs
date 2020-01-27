using System;
using System.Linq;
using System.Threading.Tasks;
using Bijou.Types;

namespace Bijou.Example.Samples
{
    public static class SampleRunner
    {
        private static readonly SampleRunnerOutput Output = new SampleRunnerOutput();

        public static IObservable<string> Stream => Output.Stream;

        public static async Task SimpleFunction()
        {
            var engine = new UWPChakraHostExecutor();
            var result = await engine.RunScriptAsync<JavaScriptNumber>(@"
                function square() { return 10 * 10; }
                square();
            ");

            Output.Write($"The square of 10 * 10 is : {(int)result.Value} (Unsafe)");

            Output.Write(
                result.IsSuccess
                ? $"The square of 10 * 10 is : {result.Value.AsInt32()} (Safe)"
                : $"Something went wrong: {result.Errors.First().Message}");
        }
    }
}
