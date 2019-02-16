using System;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

namespace JaDisco_UWP.ViewModels
{
    internal class MainPageViewModel
    {
        public async void MemoryCleanup()
        {
            await WebView.ClearTemporaryWebDataAsync();
            GC.Collect();
        }

        public async void LaunchUri(Uri uri)
        {
            _ = await Launcher.LaunchUriAsync(uri);
        }

        public void TitleBarCustomization()
        {
            CoreApplicationViewTitleBar CoreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            CoreTitleBar.ExtendViewIntoTitleBar = false;

            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ForegroundColor = Colors.White;
            titleBar.BackgroundColor = Colors.Black;
            titleBar.ButtonForegroundColor = Colors.White;
            titleBar.ButtonBackgroundColor = Colors.Black;

            titleBar.InactiveBackgroundColor = Colors.Black;
            titleBar.ButtonInactiveBackgroundColor = Colors.Black;
        }
    }
}