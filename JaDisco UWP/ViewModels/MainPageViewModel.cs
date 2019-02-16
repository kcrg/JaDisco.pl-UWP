using HtmlAgilityPack;
using System;
using Windows.System;
using Windows.UI.Xaml.Controls;

namespace JaDisco_UWP.ViewModels
{
    internal class MainPageViewModel
    {
        public string Status { get; set; }

        private string HTML { get; set; }

        private readonly WebView webView = new WebView();

        public async void MemoryCleanup()
        {
            await WebView.ClearTemporaryWebDataAsync();
            GC.Collect();
        }

        public async void LaunchUri(Uri uri)
        {
            _ = await Launcher.LaunchUriAsync(uri);
        }

        public async void StartStatusParse()
        {
            webView.NavigationCompleted += webView_NavigationCompleted;
            webView.Navigate(new Uri("https://jadisco.pl/"));     
        }

        private async void webView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (args.IsSuccess == true)
            {
                HTML = await webView.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });

                var htmlDoc = new HtmlDocument();

                htmlDoc.LoadHtml(HTML);

                //flex-navbar-item jd-title
                Status = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='flex-navbar-item jd-title']").InnerText;
            }
            else
            {

            }
        }
    }
}