using HtmlAgilityPack;
using System;
using Windows.System;
using Windows.UI.Xaml.Controls;

namespace JaDisco_UWP.ViewModels
{
    internal class MainPageViewModel
    {
        public string Status { get; set; }

        public async void MemoryCleanup()
        {
            await WebView.ClearTemporaryWebDataAsync();
            GC.Collect();
        }

        public async void LaunchUri(Uri uri)
        {
            _ = await Launcher.LaunchUriAsync(uri);
        } 
    }
}