using JaDisco_UWP.ViewModels;
using JaDisco_UWP.Views;
using System;
using System.Linq;

using Windows.ApplicationModel.Core;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

using Twitch.Api;
using Twitch.Api.Models;

using Jadisco.Api;
using Jadisco.Api.Models;

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

        private StreamQualitiesViewModel streamQualitiesVM = new StreamQualitiesViewModel();

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
                StatusTextBlock.Margin = new Thickness(90, 0, 0, 0);
            }
            else if (App.RunningOnDesktop)
            {
                vm.TitleBarCustomization();
                Window.Current.SetTitleBar(DragArea);
            }

            //NavView.SelectedItem = NavView.MenuItems[0];

            StreamMediaPlayer.MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            StreamMediaPlayer.MediaPlayer.Play();

            QualityStackPanel.DataContext = streamQualitiesVM;
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

            LoadQualityList(playlist);
            ChangeStream(playlist.Playlist[0]);
        }

        private void StopStream()
        {
            streamQualitiesVM.IsNotAvailable = true;
            streamQualitiesVM.ClearQualityList();

            StreamMediaPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/SplashAssets/SplashVideo.mp4"));
            StreamMediaPlayer.AreTransportControlsEnabled = false;
            StreamMediaPlayer.MediaPlayer.Play();
        }

        private void ChangeStream(HLSStream stream)
        {
            if (stream is null)
                return;

            StreamMediaPlayer.Source = MediaSource.CreateFromUri(new Uri(stream.Url));
            StreamMediaPlayer.MediaPlayer.Play();
            currentStream = stream;

            StreamMediaPlayer.AreTransportControlsEnabled = true;
        }

        private void LoadQualityList(HLSPlaylist playlist)
        {
            if (playlist?.Playlist != null && playlist.Playlist.Count() > 0)
            {
                streamQualitiesVM.ClearQualityList();

                foreach (var stream in playlist.Playlist)
                {
                    if (stream.Name.StartsWith("audio"))
                        continue;

                    streamQualitiesVM.AddQuality(new StreamQualityViewModel
                    {
                        Name = stream.Name,
                        Stream = stream
                    });
                }

                streamQualitiesVM.IsNotAvailable = false;
            }
        }

        private void StreamQualityButton_Checked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            var data = radio.DataContext as StreamQualityViewModel;

            if (data is null)
                return;

            ChangeStream(data.Stream);
        }

        private async void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
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
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (obj.StreamerId == 1)
                {
                    WonziuNav.Content = "Wonziu - Online";
                    WonziuNav.IsEnabled = true;
                    NavView.SelectedItem = NavView.MenuItems[0];
                }
                else if (obj.StreamerId == 2)
                {
                    DzejNav.Content = "Dzej - Online";
                    DzejNav.IsEnabled = true;
                    NavView.SelectedItem = NavView.MenuItems[1];
                }

                ChangeStream(obj.Id);
            });

            // if stream is already online and new streamer isn't Wonziu (Wonziu has priority)
            //if (jadiscoApi.Stream.Status == true && obj.StreamerId != 1)
            //{
            //    return;
            //}
        }

        private async void JadiscoApi_OnStreamWentOffline(Service obj)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (obj.StreamerId == 1)
                {
                    WonziuNav.Content = "Wonziu - Offline";
                    WonziuNav.IsEnabled = false;
                    WonziuNav.IsSelected = false;
                }
                else if (obj.StreamerId == 2)
                {
                    DzejNav.Content = "Dzej - Offline";
                    DzejNav.IsEnabled = false;
                    DzejNav.IsSelected = false;
                }

                StopStream();
            });

            streamPlaylist = null;
            currentStream = null;
        }

        private async void JadiscoApi_OnTopicChanged(Topic topic)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                StatusTextBlock.Text = topic.Text.Trim();

                StatusFlyoutTextBlock.Text = topic.Text.Trim();
                StatusDateFlyoutTextBlock.Text = "Dodane: " + topic.UpdatedAt.ToString().Replace(" +00:00", "");
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

                StreamGrid.SetValue(Grid.ColumnProperty, 1);
                ChatGrid.SetValue(Grid.ColumnProperty, 0);

                LeftColumn.Width = new GridLength(2300, GridUnitType.Star);
                RightColumn.Width = new GridLength(7700, GridUnitType.Star);

                LeftChat = true;
            }
            else if (LeftChat)
            {
                ChatPositionIcon.Glyph = "";

                StreamGrid.SetValue(Grid.ColumnProperty, 0);
                ChatGrid.SetValue(Grid.ColumnProperty, 1);

                LeftColumn.Width = new GridLength(7700, GridUnitType.Star);
                RightColumn.Width = new GridLength(2300, GridUnitType.Star);

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
                switch (args.SelectedItemContainer.Tag)
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
                            Streamer streamer = jadiscoApi.Streamers?.SingleOrDefault(m => m.Name == "dzej");

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

        private void DonateWonziu_Click(object sender, RoutedEventArgs e)
        {
            vm.LaunchUri("https://streamlabs.com/wonziu");
        }
        private void Donate_Tapped(object sender, TappedRoutedEventArgs e)
        {
            vm.LaunchUri("https://tinyurl.com/DonateMohairApps");
        }

        private void Github_Tapped(object sender, TappedRoutedEventArgs e)
        {
            vm.LaunchUri("https://github.com/kcrg/JaDisco.pl-UWP");
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
                chatHideToolTip.Content = "Pokaż czat";
                ToolTipService.SetToolTip(ChatHideButton, chatHideToolTip);

                if (!LeftChat)
                {
                    RightColumn.Width = new GridLength(0);
                    ChatGrid.Visibility = Visibility.Collapsed;
                    ChatPositionButton.IsEnabled = false;

                    ChatWebView.Navigate(BlankUri);
                    GC.Collect();
                }
                else if (LeftChat)
                {
                    LeftColumn.Width = new GridLength(0);
                    ChatGrid.Visibility = Visibility.Collapsed;
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

                if (fromHideButton)
                {
                    if (!LeftChat)
                    {
                        RightColumn.Width = new GridLength(2300, GridUnitType.Star);
                        ChatGrid.Visibility = Visibility.Visible;
                        ChatPositionButton.IsEnabled = true;

                        ChatWebView.Navigate(ChatUri);
                    }
                    else if (LeftChat)
                    {
                        LeftColumn.Width = new GridLength(2300, GridUnitType.Star);
                        ChatGrid.Visibility = Visibility.Visible;
                        ChatPositionButton.IsEnabled = true;

                        ChatWebView.Navigate(ChatUri);
                    }

                    HiddenChat = false;
                }
            }
        }
    }
}
