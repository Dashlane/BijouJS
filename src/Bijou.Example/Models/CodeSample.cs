using System;
using System.Threading.Tasks;

namespace Bijou.Example.Models
{
    public class CodeSample
    {
        public string Title { get; }

        public Uri File { get; }

        public Func<Task> Execution { get; }

        public CodeSample(string title, Uri file, Func<Task> execution)
        {
            Title = title;
            File = file;
            Execution = execution;
        }
    }
}