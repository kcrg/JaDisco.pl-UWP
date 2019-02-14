using System;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace JaDisco_UWP
{
    public sealed partial class MainPage : Page
    {
        private bool LeftChat, HiddenChat = false;
        private readonly Uri ChatUri = new Uri("https://client.poorchat.net/jadisco");
        private readonly Uri StreamUri = new Uri("https://player.twitch.tv/?channel=wonziu");
        private readonly ToolTip chatHideToolTip = new ToolTip();

        private readonly ApplicationView view = ApplicationView.GetForCurrentView();
        //Debug.WriteLine(ChatHideIcon.Glyph);
        public MainPage()
        {
            InitializeComponent();
            //StatusParser(); // add stream status parsing

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

        private async void ChatHideButton_Click(object sender, RoutedEventArgs e)
        {
            if (HiddenChat == false)
            {
                ChatHideIcon.Glyph = "";
                chatHideToolTip.Content = "Pokaż chat";
                ToolTipService.SetToolTip(ChatHideButton, chatHideToolTip);

                if (LeftChat == false)
                {
                    RightColumn.Width = new GridLength(0);
                    RightColumn.MinWidth = 0;
                    ChatWebView.Visibility = Visibility.Collapsed;
                    ChatPositionButton.IsEnabled = false;

                    ChatWebView.Navigate(new Uri("about:blank"));
                    await WebView.ClearTemporaryWebDataAsync();
                    GC.Collect();
                }
                else if (LeftChat == true)
                {
                    LeftColumn.Width = new GridLength(0);
                    LeftColumn.MinWidth = 0;
                    ChatWebView.Visibility = Visibility.Collapsed;
                    ChatPositionButton.IsEnabled = false;

                    ChatWebView.Navigate(new Uri("about:blank"));
                    await WebView.ClearTemporaryWebDataAsync();
                    GC.Collect();
                }

                HiddenChat = true;
            }
            else if (HiddenChat == true)
            {
                ChatHideIcon.Glyph = "";
                chatHideToolTip.Content = "Schowaj chat";
                ToolTipService.SetToolTip(ChatHideButton, chatHideToolTip);

                if (LeftChat == false)
                {
                    RightColumn.Width = new GridLength(20, GridUnitType.Star);
                    RightColumn.MinWidth = 200;
                    ChatWebView.Visibility = Visibility.Visible;
                    ChatPositionButton.IsEnabled = true;

                    ChatWebView.Navigate(ChatUri);
                }
                else if (LeftChat == true)
                {
                    LeftColumn.Width = new GridLength(20, GridUnitType.Star);
                    LeftColumn.MinWidth = 200;
                    ChatWebView.Visibility = Visibility.Visible;
                    ChatPositionButton.IsEnabled = true;

                    ChatWebView.Navigate(ChatUri);
                }

                HiddenChat = false;
            }
        }

        private void ChatPositionButton_Click(object sender, RoutedEventArgs e)
        {
            if (LeftChat == false)
            {
                ChatPositionIcon.Glyph = "";

                StreamWebView.SetValue(Grid.ColumnProperty, 1);
                ChatWebView.SetValue(Grid.ColumnProperty, 0);

                LeftColumn.Width = new GridLength(20, GridUnitType.Star);
                LeftColumn.MinWidth = 200;
                RightColumn.Width = new GridLength(80, GridUnitType.Star);

                LeftChat = true;
            }
            else if (LeftChat == true)
            {
                ChatPositionIcon.Glyph = "";

                StreamWebView.SetValue(Grid.ColumnProperty, 0);
                ChatWebView.SetValue(Grid.ColumnProperty, 1);

                LeftColumn.Width = new GridLength(80, GridUnitType.Star);
                LeftColumn.MinWidth = 0;
                RightColumn.Width = new GridLength(20, GridUnitType.Star);
                RightColumn.MinWidth = 200;

                LeftChat = false;
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (view.IsFullScreenMode == true)
            {
                TopCommandBar.Visibility = Visibility.Collapsed;
                TopRow.Height = new GridLength(0);
            }
            else if (view.IsFullScreenMode == false)
            {
                TopRow.Height = new GridLength(36);
                TopCommandBar.Visibility = Visibility.Visible;
            }
        }

        private void StreamWebView_ContainsFullScreenElementChanged(WebView sender, object args) //TODO: nie działa maybe useragent :c
        {
            if (sender.ContainsFullScreenElement)
            {
                view.TryEnterFullScreenMode();
                TopCommandBar.Visibility = Visibility.Collapsed;
            }
            else if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
                TopCommandBar.Visibility = Visibility.Visible;
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            StreamWebView.Source = StreamUri;
            StreamWebView.Refresh();

            ChatWebView.Source = ChatUri;
            ChatWebView.Refresh();

            await WebView.ClearTemporaryWebDataAsync();
            GC.Collect();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private async void Facebook_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _ = await Launcher.LaunchUriAsync(new Uri(@"https://www.facebook.com/VersatileSoftware"));
        }

        private async void Wykop_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _ = await Launcher.LaunchUriAsync(new Uri(@"https://www.wykop.pl/tag/jadiscouwp/"));
        }

        private async void StreamWebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            await WebView.ClearTemporaryWebDataAsync();
            GC.Collect();
        }

        private async void ChatWebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            await WebView.ClearTemporaryWebDataAsync();
            GC.Collect();
        }

        private async void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            _ = await Launcher.LaunchUriAsync(new Uri(@"https://streamlabs.com/wonziu"));
        }

        //private async void StatusParser()
        //{
        //    HtmlDocument doc = new HtmlWeb().Load("https://jadisco.pl/");
        //    //string div = doc.DocumentNode.SelectSingleNode("//div[@class='flex-navbar-item jd-title']//text()[normalize-space()]").InnerText;

        //    var divs = doc.DocumentNode.SelectNodes("//div[contains(@class,'flex-navbar')]");
        //    foreach (HtmlNode div in divs)
        //    {
        //        var node = div.SelectSingleNode("text()[normalize-space()]");
        //        StatusTextBlock.Text = node.InnerText.Trim();
        //    }

        //    //string html = await GetResponseFromURI(new Uri("https://jadisco.pl/"));

        //    //HtmlDocument doc = new HtmlDocument();
        //    //doc.LoadHtml(html);
        //    //HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class=\"flex-navbar-item jd-title\"]");
        //    //if (nodes != null)
        //    //{
        //    //    StatusTextBlock.Text = nodes.Select(node => node.InnerText).FirstOrDefault();
        //    //}

        //    //Debug.WriteLine(node + "   asdasdad");
        //    //StatusTextBlock.Text = nodes.ToString();
        //}

        //private static async Task<string> GetResponseFromURI(Uri uri)
        //{
        //    string response = "";
        //    using (HttpClient client = new HttpClient())
        //    {
        //        HttpResponseMessage result = await client.GetAsync(uri);
        //        if (result.IsSuccessStatusCode)
        //        {
        //            response = await result.Content.ReadAsStringAsync();
        //        }
        //    }
        //    return response;
        //}
    }
}