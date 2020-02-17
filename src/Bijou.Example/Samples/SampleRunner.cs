using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bijou.Example.Samples
{
    public static class SampleRunner
    {
        private static readonly SampleRunnerOutput Output = new SampleRunnerOutput();

        public static IObservable<string> Stream => Output.Stream;

        public static async Task SimpleFunction()
        {
            var engine = new BijouExecutor();
            var result = await engine.RunScriptAsync(@"
                function square() { return 10 * 10; }
                square();
            ");

            Output.Write(
                result.IsSuccess
                ? $"The script has been executed successfully"
                : $"Something went wrong: {result.Errors.First().Message}");
        }
    }
}
