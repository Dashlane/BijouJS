var engine = new UWPChakraHostExecutor();
var result = await engine.RunScriptAsync<JavaScriptNumber>(@"
    function square() { return 10 * 10; }
    square();
");

Console.WriteLine($"The square of 10 * 10 is : {(int)result.Value} (Unsafe)");

if (result.IsSuccess)
{
    Console.WriteLine($"The square of 10 * 10 is : {result.Value.AsInt32()} (Safe)");
}
else
{
    Console.WriteLine($"Something went wrong: {result.Errors.First().Message}");
}