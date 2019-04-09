using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

namespace JaDisco_UWP.Views
{
    public sealed partial class ChatPage : Page
    {
        public ChatPage()
        {
            InitializeComponent();
            TitlebarCustomizations();
        }

        private void TitlebarCustomizations()
        {
            if (App.RunningOnDesktop)
            {
                CoreApplicationViewTitleBar CoreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                CoreTitleBar.ExtendViewIntoTitleBar = true;

                ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonForegroundColor = Colors.White;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                titleBar.ButtonHoverForegroundColor = Colors.White;
                titleBar.ButtonHoverBackgroundColor = Colors.Black;
                titleBar.BackgroundColor = Colors.Black;
            }
        }
    }
}