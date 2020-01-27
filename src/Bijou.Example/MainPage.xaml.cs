using Windows.UI.Xaml.Controls;
using ReactiveUI;

namespace Bijou.Example
{
    public abstract class BaseMainPage : ReactivePage<MainPageViewModel> { }

    public sealed partial class MainPage : BaseMainPage
    {
        public MainPage()
        {
            InitializeComponent();

            ViewModel = new MainPageViewModel();
        }
    }
}
