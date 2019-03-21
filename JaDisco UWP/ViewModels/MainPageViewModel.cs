using System;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace JaDisco_UWP.ViewModels
{
    internal class MainPageViewModel
    {
        public void MemoryCleanup()
        {
            GC.Collect();
        }

        public async void LaunchUri(string uri)
        {
            _ = await Launcher.LaunchUriAsync(new Uri(uri));
        }

        public void TitleBarCustomization()
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