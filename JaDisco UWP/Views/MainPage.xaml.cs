using Jadisco.Api;
using Jadisco.Api.Models;
using JaDisco_UWP.ViewModels;
using JaDisco_UWP.Views;
using JaDisco_UWP.Views.CustomDialogs;
using System;
using System.Linq;
using Twitch.Api;
using Twitch.Api.Models;
using Windows.ApplicationModel.Core;
using Windows.Media.Core;
using Windows.UI.Core;
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
        private bool LeftChat, HiddenChat = false;
        private readonly Uri ChatUri = new Uri("https://client.poorchat.net/jadisco");
        private readonly Uri BlankUri = new Uri("about:blank");

        private readonly ToolTip chatHideToolTip = new ToolTip();
        private readonly MainPageViewModel vm = new MainPageViewModel();

        private readonly JadiscoApi jadiscoApi = new JadiscoApi();
        private HLSPlaylist streamPlaylist = null;
        private HLSStream currentStream = null;

        public MainPage()
        {
            jadiscoApi.OnTopicChanged += JadiscoApi_OnTopicChanged;
            jadiscoApi.OnStreamWentOnline += JadiscoApi_OnStreamWentOnline;
            jadiscoApi.OnStreamWentOffline += JadiscoApi_OnStreamWentOffline;
            jadiscoApi.Connect();

            InitializeComponent();

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
            NavView.SelectedItem = NavView.MenuItems[0];

            StreamMediaPlayer.MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            StreamMediaPlayer.MediaPlayer.Play();
        }

        private void ChangeStream(string channel)
        {
            AccessToken token = TwitchApi.GetAccessToken(channel);

            if (token is null)
            {
                return;
            }

            string url = UsherService.GetStreamLink(channel, token.Sig, token.Token);

            HLSPlaylist playlist = UsherService.ParsePlaylists(url);

            if (playlist is null)
            {
                return;
            }

            streamPlaylist = playlist;

            ChangeStream(playlist.Playlist[0]);
        }

        private void ChangeStream(HLSStream stream)
        {
            StreamMediaPlayer.Source = MediaSource.CreateFromUri(new Uri(stream.Url));
            StreamMediaPlayer.MediaPlayer.Play();
            currentStream = stream;
        }

        private async void MediaPlayer_MediaEnded(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            if (jadiscoApi.Stream.Status == false)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    StreamMediaPlayer.MediaPlayer.Play();
                });
            }
        }

        private async void JadiscoApi_OnStreamWentOnline(Service obj)
        {
            // if stream is already online and new streamer isn't Wonziu (Wonziu has priority)
            if (jadiscoApi.Stream.Status == true && obj.StreamerId != 1)
            {
                return;
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ChangeStream(obj.Id);
                StreamMediaPlayer.AreTransportControlsEnabled = true;
            });
        }

        private async void JadiscoApi_OnStreamWentOffline(Service obj)
        {
            streamPlaylist = null;
            currentStream = null;

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                StreamMediaPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/SplashAssets/SplashVideo.mp4"));
                StreamMediaPlayer.AreTransportControlsEnabled = false;
                StreamMediaPlayer.MediaPlayer.Play();
            });
        }

        private async void JadiscoApi_OnTopicChanged(Topic topic)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                StatusTextBlock.Text = topic.Text;
            });
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
            ChatWebView.Source = ChatUri;
            ChatWebView.Refresh();

            if (currentStream != null)
            {
                ChangeStream(currentStream);
            }
        }

        private void NavView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                switch (args.SelectedItemContainer.Content)
                {
                    case "Wonziu":
                        {
                            Streamer streamer = jadiscoApi.Streamers?.SingleOrDefault(m => m.Name == "Wonziu");

                            if (streamer is null)
                            {
                                break;
                            }

                            Service stream = jadiscoApi.Stream?.Services?.FirstOrDefault(m => m.StreamerId == streamer.Id && m.Status == true);

                            if (stream is null)
                            {
                                break;
                            }

                            ChangeStream(stream.Id);
                        }
                        break;
                    case "Dzej":
                        {
                            Streamer streamer = jadiscoApi.Streamers?.SingleOrDefault(m => m.Name == "Dzej");

                            if (streamer is null)
                            {
                                break;
                            }

                            Service stream = jadiscoApi.Stream?.Services?.FirstOrDefault(m => m.StreamerId == streamer.Id && m.Status == true);

                            if (stream is null)
                            {
                                break;
                            }

                            ChangeStream(stream.Id);
                        }
                        break;
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
                ;
                chatHideToolTip.Content = "Pokaż czat";
                ToolTipService.SetToolTip(ChatHideButton, chatHideToolTip);

                if (!LeftChat)
                {
                    RightColumn.Width = new GridLength(0);
                    RightColumn.MinWidth = 0;
                    ChatWebView.Visibility = Visibility.Collapsed;
                    ChatPositionButton.IsEnabled = false;

                    ChatWebView.Navigate(BlankUri);
                    GC.Collect();
                }
                else if (LeftChat)
                {
                    LeftColumn.Width = new GridLength(0);
                    LeftColumn.MinWidth = 0;
                    ChatWebView.Visibility = Visibility.Collapsed;
                    ChatPositionButton.IsEnabled = false;

                    ChatWebView.Navigate(BlankUri);
                    GC.Collect();
                }

                HiddenChat = true;
            }
            else if (HiddenChat)
            {
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
    }
}