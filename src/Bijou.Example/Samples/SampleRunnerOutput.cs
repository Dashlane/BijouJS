using System;
using System.Reactive.Subjects;

namespace Bijou.Example.Samples
{
    public class SampleRunnerOutput
    {
        private readonly Subject<string> _output = new Subject<string>();

        public IObservable<string> Stream => _output;

        public void Write(string text)
        {
            _output.OnNext(text);
        }
    }
}
