using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Bijou.Example.Models;
using Bijou.Example.Samples;
using Bijou.Example.ViewModels;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Bijou.Example
{
    public class MainPageViewModel : ReactiveObject
    {
        private readonly CollectionViewSource _samples = new CollectionViewSource();
        private readonly SourceList<string> _outputs = new SourceList<string>();

        public ICollectionView Samples => _samples.View;

        public extern CodeSampleViewModel SelectedSample { [ObservableAsProperty] get; }

        public IObservableCollection<string> Outputs { get; } = new ObservableCollectionExtended<string>();

        public MainPageViewModel()
        {
            _samples.Source = new List<CodeSample>
            {
                new CodeSample(
                    "Simple function",
                    new Uri("Samples/Code/SimpleFunction.cs", UriKind.Relative),
                    SampleRunner.SimpleFunction)
            };

            this.WhenAnyValue(c => c.Samples.CurrentItem)
                .Select(c => new CodeSampleViewModel(c as CodeSample))
                .ToPropertyEx(this, c => c.SelectedSample);

            SampleRunner.Stream.Subscribe(c => _outputs.Add(c));

            _outputs.Connect()
                    .Bind(Outputs)
                    .Subscribe();
        }
    }
}
