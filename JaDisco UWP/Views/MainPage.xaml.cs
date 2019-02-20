using JaDisco_UWP.ViewModels;
using JaDisco_UWP.Views;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Jadisco.Api;
using Jadisco.Api.Data;

namespace JaDisco_UWP
{
    public sealed partial class MainPage : Page
    {
        private bool LeftChat, HiddenChat, IsStatusParse = false;
        private readonly Uri ChatUri = new Uri("https://client.poorchat.net/jadisco");
        private readonly Uri WonziuUri = new Uri("https://player.twitch.tv/?channel=wonziu");
        private readonly Uri DzejUri = new Uri("https://player.twitch.tv/?channel=dzejth");
        private readonly Uri BlankUri = new Uri("about:blank");

        private readonly WebView statusWebView = new WebView();
        private readonly ToolTip chatHideToolTip = new ToolTip();
        private readonly MainPageViewModel vm = new MainPageViewModel();

        private readonly JadiscoApi jadiscoApi = new JadiscoApi();

        public MainPage()
        {
            jadiscoApi.OnTopicChanged += JadiscoApi_OnTopicChanged;
            jadiscoApi.Connect();

            InitializeComponent();
            StartStatusParse();

            if (App.RunningOnXbox || App.RunningOnMobile)
            {
                ChatInNewWindow.Visibility = Visibility.Collapsed;
            }
            else
            {
                vm.TitleBarCustomization();
                Window.Current.SetTitleBar(DragArea);
            }

            NavigationCacheMode = NavigationCacheMode.Required;
        }

        private async void JadiscoApi_OnTopicChanged(Topic topic)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                StatusTextBlock.Text = topic.Text;
            });
        

            //NavView.SelectedItem = NavView.MenuItems[0];

            //var token = TwitchApi.GetAccessToken("lirik");
            //var url = UsherService.GetStreamLink("lirik", token.Sig, token.Token);
            //var playlist = UsherService.ParsePlaylists(url);

            //StreamMediaPlayer.Source = MediaSource.CreateFromUri(new Uri(playlist.Playlist[0].Url));
        }

        private void ChatHideButton_Click(object sender, RoutedEventArgs e)
        {
            HideChat(true);
        }

        private void ChatPositionButton_Click(object sender, RoutedEventArgs e)
        {
            if (!LeftChat)
            {
                ChatPositionIcon.Glyph = "";

                StreamMediaPlayer.SetValue(Grid.ColumnProperty, 1);
                ChatWebView.SetValue(Grid.ColumnProperty, 0);

                LeftColumn.Width = new GridLength(20, GridUnitType.Star);
                LeftColumn.MinWidth = 200;
                RightColumn.Width = new GridLength(80, GridUnitType.Star);
                RightColumn.MinWidth = 0;

                LeftChat = true;
            }
            else if (LeftChat)
            {
                ChatPositionIcon.Glyph = "";

                StreamMediaPlayer.SetValue(Grid.ColumnProperty, 0);
                ChatWebView.SetValue(Grid.ColumnProperty, 1);

                LeftColumn.Width = new GridLength(80, GridUnitType.Star);
                LeftColumn.MinWidth = 0;
                RightColumn.Width = new GridLength(20, GridUnitType.Star);
                RightColumn.MinWidth = 200;

                LeftChat = false;
            }
        }

        //private void StreamWebView_ContainsFullScreenElementChanged(WebView sender, object args)
        //{
        //    ApplicationView view = ApplicationView.GetForCurrentView();

        //    if (sender.ContainsFullScreenElement)
        //    {
        //        view.TryEnterFullScreenMode();
        //        NavView.IsPaneVisible = false;
        //        NavView.Margin = new Thickness(0);
        //        DragArea.Visibility = Visibility.Collapsed;
        //    }
        //    else if (view.IsFullScreenMode)
        //    {
        //        view.ExitFullScreenMode();
        //        NavView.Margin = new Thickness(0, 38, 0, 0);
        //        NavView.IsPaneVisible = true;
        //        DragArea.Visibility = Visibility.Visible;
        //    }
        //}

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            //StreamWebView.Refresh();

            ChatWebView.Source = ChatUri;
            ChatWebView.Refresh();

            IsStatusParse = false;
        }

        private void NavView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                if (args.SelectedItemContainer != null)
                {
                    switch (args.SelectedItemContainer.Content)
                    {
                        case "Wonziu":
                            //StreamWebView.Navigate(WonziuUri);
                            break;

                        case "Dzej":
                            //StreamWebView.Navigate(DzejUri);
                            break;
                    }
                }
            }
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

        private async void ChatNewWindowButton_Click(object sender, RoutedEventArgs e)
        {
            CoreApplicationView newView = CoreApplication.CreateNewView();
            int newViewId = 0;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ApplicationView newAppView = ApplicationView.GetForCurrentView();
                newAppView.Title = "Czat";

                Frame frame = new Frame();
                frame.Navigate(typeof(ChatPage), null);
                Window.Current.Content = frame;
                // You have to activate the window in order to show it later.
                Window.Current.Activate();

                newViewId = ApplicationView.GetForCurrentView().Id;
            });
            bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);

            HideChat(false);
        }

        private void HideChat(bool fromHideButton)
        {
            if (!HiddenChat)
            {
                //ChatHideIcon.Glyph = "";
                chatHideToolTip.Content = "Pokaż czat";
                ToolTipService.SetToolTip(ChatHideButton, chatHideToolTip);

                if (!LeftChat)
                {
                    RightColumn.Width = new GridLength(0);
                    RightColumn.MinWidth = 0;
                    ChatWebView.Visibility = Visibility.Collapsed;
                    ChatPositionButton.IsEnabled = false;

                    ChatWebView.Navigate(BlankUri);
                }
                else if (LeftChat)
                {
                    LeftColumn.Width = new GridLength(0);
                    LeftColumn.MinWidth = 0;
                    ChatWebView.Visibility = Visibility.Collapsed;
                    ChatPositionButton.IsEnabled = false;

                    ChatWebView.Navigate(BlankUri);
                }

                HiddenChat = true;
            }
            else if (HiddenChat)
            {
                //ChatHideIcon.Glyph = "";
                chatHideToolTip.Content = "Schowaj czat";
                ToolTipService.SetToolTip(ChatHideButton, chatHideToolTip);

                if (!LeftChat)
                {
                    if (fromHideButton)
                    {
                        RightColumn.Width = new GridLength(20, GridUnitType.Star);
                        RightColumn.MinWidth = 250;
                        ChatWebView.Visibility = Visibility.Visible;
                        ChatPositionButton.IsEnabled = true;

                        ChatWebView.Navigate(ChatUri);
                    }
                }
                else if (LeftChat)
                {
                    if (fromHideButton)
                    {
                        LeftColumn.Width = new GridLength(20, GridUnitType.Star);
                        LeftColumn.MinWidth = 250;
                        ChatWebView.Visibility = Visibility.Visible;
                        ChatPositionButton.IsEnabled = true;

                        ChatWebView.Navigate(ChatUri);
                    }
                }

                HiddenChat = false;
            }
        }

        //private async void webView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        //{
        //    if (args.IsSuccess == true)
        //    {
        //        if (IsStatusParse == false)
        //        {
        //            string HTML = await statusWebView.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });
        //            HtmlDocument htmlDoc = new HtmlDocument();
        //            htmlDoc.LoadHtml(HTML);

        //            //flex-navbar-item jd-title
        //            StatusTextBlock.Text = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='flex-navbar-item jd-title']").InnerText;
        //            ProgressBar.IsIndeterminate = false;

        //            IsStatusParse = true;
        //            statusWebView.Navigate(BlankUri);

        //            vm.MemoryCleanup();
        //        }
        //    }
        //}

        //private void webView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        //{
        //    if (IsStatusParse == false)
        //    {
        //        StatusTextBlock.Text = "Ładowanie statusu...";
        //        ProgressBar.IsIndeterminate = true;
        //    }
        //}
    }
}