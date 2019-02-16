using HtmlAgilityPack;
using JaDisco_UWP.ViewModels;
using System;
using Windows.ApplicationModel.Core;
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
        private readonly WebView webView = new WebView();

        private readonly MainPageViewModel vm = new MainPageViewModel();

        public MainPage()
        {
            InitializeComponent();
            StartStatusParse();

            CoreApplicationViewTitleBar CoreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            CoreTitleBar.ExtendViewIntoTitleBar = false;

            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ForegroundColor = Colors.White;
            titleBar.BackgroundColor = Colors.Black;
            titleBar.ButtonForegroundColor = Colors.White;
            titleBar.ButtonBackgroundColor = Colors.Black;

            titleBar.InactiveBackgroundColor = Colors.Black;
            titleBar.ButtonInactiveBackgroundColor = Colors.Black;

            webView.NavigationCompleted += webView_NavigationCompleted;
            webView.NavigationStarting += webView_NavigationStarting;
        }

        private void ChatHideButton_Click(object sender, RoutedEventArgs e)
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
                }
                else if (LeftChat == true)
                {
                    LeftColumn.Width = new GridLength(0);
                    LeftColumn.MinWidth = 0;
                    ChatWebView.Visibility = Visibility.Collapsed;
                    ChatPositionButton.IsEnabled = false;

                    ChatWebView.Navigate(new Uri("about:blank"));
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

        private void StreamWebView_ContainsFullScreenElementChanged(WebView sender, object args)
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

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            StreamWebView.Source = StreamUri;
            StreamWebView.Refresh();

            ChatWebView.Source = ChatUri;
            ChatWebView.Refresh();

            StartStatusParse();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            vm.MemoryCleanup();
        }

        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            vm.LaunchUri(new Uri(@"https://streamlabs.com/wonziu"));
        }

        private void Facebook_Tapped(object sender, TappedRoutedEventArgs e)
        {
            vm.LaunchUri(new Uri(@"https://www.facebook.com/VersatileSoftware"));
        }

        private void Wykop_Tapped(object sender, TappedRoutedEventArgs e)
        {
            vm.LaunchUri(new Uri(@"https://www.wykop.pl/tag/jadiscouwp/"));
        }

        private void StartStatusParse()
        {
            webView.Navigate(new Uri("https://jadisco.pl/"));
        }

        private async void webView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (args.IsSuccess == true)
            {
                string HTML = await webView.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });

                HtmlDocument htmlDoc = new HtmlDocument();

                htmlDoc.LoadHtml(HTML);

                //flex-navbar-item jd-title
                string Status = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='flex-navbar-item jd-title']").InnerText;

                StatusTextBlock.Text = Status;
                ProgressBar.Visibility = Visibility.Collapsed;
                vm.MemoryCleanup();
            }
        }

        private void webView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            ProgressBar.Visibility = Visibility.Visible;
        }
    }
}