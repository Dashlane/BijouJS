using System.IO;
using System.Reactive;
using System.Text;
using System.Windows.Input;
using Bijou.Example.Models;
using ReactiveUI;

namespace Bijou.Example.ViewModels
{
    public sealed class CodeSampleViewModel : ReactiveObject
    {
        private readonly ReactiveCommand<Unit, Unit> _run;
        private readonly string _code;

        public string Code { get; }

        public ICommand Run => _run;

        public CodeSampleViewModel(CodeSample sample)
        {
            _code = File.ReadAllText(sample.File.OriginalString);
            Code = new StringBuilder().AppendLine("```csharp")
                                      .AppendLine(_code)
                                      .AppendLine("```")
                                      .ToString();

            _run = ReactiveCommand.CreateFromTask(sample.Execution);
        }
    }
}