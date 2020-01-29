var engine = new UWPChakraHostExecutor();
var result = await engine.RunScriptAsync(@"
                function square() { return 10 * 10; }
                square();
            ");

Output.Write(
result.IsSuccess
? $"The script has been executed successfully"
: $"Something went wrong: {result.Errors.First().Message}");