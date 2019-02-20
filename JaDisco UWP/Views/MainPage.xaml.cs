using HtmlAgilityPack;
using JaDisco_UWP.ViewModels;
using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace JaDisco_UWP
{
    public sealed partial class MainPage : Page
    {
        private bool LeftChat, HiddenChat, IsStatusParse = false;
        private readonly Uri ChatUri = new Uri("https://client.poorchat.net/jadisco");
        private readonly Uri WonziuUri = new Uri("https://player.twitch.tv/?channel=wonziu");
        private readonly Uri DzejUri = new Uri("https://player.twitch.tv/?channel=dzejth");
        private readonly Uri BlankUri = new Uri("about:blank");

        private readonly ApplicationView view = ApplicationView.GetForCurrentView();

        private readonly WebView statusWebView = new WebView();
        private readonly ToolTip chatHideToolTip = new ToolTip();
        private readonly MainPageViewModel vm = new MainPageViewModel();

        public MainPage()
        {
            InitializeComponent();
            StartStatusParse();
            vm.TitleBarCustomization();

            NavigationCacheMode = NavigationCacheMode.Required;
            Window.Current.SetTitleBar(DragArea);

            statusWebView.NavigationCompleted += webView_NavigationCompleted;
            statusWebView.NavigationStarting += webView_NavigationStarting;

            NavView.SelectedItem = NavView.MenuItems[0];

            //var token = TwitchApi.GetAccessToken("lirik");
            //var url = UsherService.GetStreamLink("lirik", token.Sig, token.Token);
            //var playlist = UsherService.ParsePlaylists(url);

            //StreamMediaPlayer.Source = MediaSource.CreateFromUri(new Uri(playlist.Playlist[0].Url));
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

                    ChatWebView.Navigate(BlankUri);
                }
                else if (LeftChat == true)
                {
                    LeftColumn.Width = new GridLength(0);
                    LeftColumn.MinWidth = 0;
                    ChatWebView.Visibility = Visibility.Collapsed;
                    ChatPositionButton.IsEnabled = false;

                    ChatWebView.Navigate(BlankUri);
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
                    RightColumn.MinWidth = 250;
                    ChatWebView.Visibility = Visibility.Visible;
                    ChatPositionButton.IsEnabled = true;

                    ChatWebView.Navigate(ChatUri);
                }
                else if (LeftChat == true)
                {
                    LeftColumn.Width = new GridLength(20, GridUnitType.Star);
                    LeftColumn.MinWidth = 250;
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
                LeftColumn.MinWidth = 250;
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
                RightColumn.MinWidth = 250;

                LeftChat = false;
            }
        }

        private void StreamWebView_ContainsFullScreenElementChanged(WebView sender, object args)
        {
            if (sender.ContainsFullScreenElement)
            {
                view.TryEnterFullScreenMode();
                NavView.IsPaneVisible = false;
                NavView.Margin = new Thickness(0);
                DragArea.Visibility = Visibility.Collapsed;
            }
            else if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
                NavView.Margin = new Thickness(0, 38, 0, 0);
                NavView.IsPaneVisible = true;
                DragArea.Visibility = Visibility.Visible;

            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            StreamWebView.Refresh();

            ChatWebView.Source = ChatUri;
            ChatWebView.Refresh();

            IsStatusParse = false;
            StartStatusParse();
        }

        private void ShowFlyout_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            vm.LaunchUri("https://streamlabs.com/wonziu");
        }
        private void Donate_Tapped(object sender, TappedRoutedEventArgs e)
        {
            vm.LaunchUri("https://tinyurl.com/DonateMohairApps");
        }

        private void Wykop_Tapped(object sender, TappedRoutedEventArgs e)
        {
            vm.LaunchUri("https://www.wykop.pl/tag/jadiscouwp/");
        }

        private void StartStatusParse()
        {
            statusWebView.Navigate(new Uri("https://jadisco.pl/"));
        }

        private void NavView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                if (args.SelectedItemContainer != null)
                {
                    switch (args.SelectedItemContainer.Tag)
                    {
                        case "Wonziu":
                            StreamWebView.Navigate(WonziuUri);
                            break;

                        case "Dzej":
                            StreamWebView.Navigate(DzejUri);
                            break;
                    }
                }
            }
        }

        private async void webView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (args.IsSuccess == true)
            {
                if (IsStatusParse == false)
                {
                    string HTML = await statusWebView.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });
                    HtmlDocument htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(HTML);

                    //flex-navbar-item jd-title
                    StatusTextBlock.Text = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='flex-navbar-item jd-title']").InnerText;
                    ProgressBar.IsIndeterminate = false;

                    IsStatusParse = true;
                    statusWebView.Navigate(BlankUri);

                    vm.MemoryCleanup();
                }
            }
        }

        private void webView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (IsStatusParse == false)
            {
                StatusTextBlock.Text = "Ładowanie statusu...";
                ProgressBar.IsIndeterminate = true;
            }
        }
    }
}