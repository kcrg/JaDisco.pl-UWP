using JaDisco_UWP.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JaDisco_UWP.Views
{
    public sealed partial class ChatPage : Page
    {
        private readonly MainPageViewModel vm = new MainPageViewModel();
        public ChatPage()
        {
            InitializeComponent();
            vm.TitleBarCustomization();
            Window.Current.SetTitleBar(DragArea);
        }
    }
}