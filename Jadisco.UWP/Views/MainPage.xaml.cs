using Jadisco.UWP.ViewModels;
using Jadisco.UWP.Views;
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

using Twitch.Api;
using Twitch.Api.Models;

using Jadisco.Api;
using Jadisco.Api.Models;
using Windows.System;

namespace Jadisco.UWP
{
    public sealed partial class MainPage : Page
    {
        private bool LeftChat, HiddenChat = false;
        private readonly Uri ChatUri = new Uri("https://client.poorchat.net/jadisco");
        private readonly Uri BlankUri = new Uri("about:blank");

        private readonly ToolTip chatHideToolTip = new ToolTip();

        private readonly JadiscoApi jadiscoApi = new JadiscoApi();
        private HLSPlaylist streamPlaylist = null;
        private HLSStream currentStream = null;
        private Service currentService = null;

        private readonly MainPageViewModel mainPageVM;

        public MainPage()
        {
            InitializeComponent();

            mainPageVM = new MainPageViewModel(this);
            DataContext = mainPageVM;

            jadiscoApi.OnTopicChanged += JadiscoApi_OnTopicChanged;
            jadiscoApi.OnStreamWentOnline += JadiscoApi_OnStreamWentOnline;
            jadiscoApi.OnStreamWentOffline += JadiscoApi_OnStreamWentOffline;
            jadiscoApi.Connect();

            if (App.RunningOnXbox || App.RunningOnMobile)
            {
                ChatInNewWindow.Visibility = Visibility.Collapsed;
                StatusTextBlock.Margin = new Thickness(90, 0, 0, 0);
            }

            StreamMediaPlayer.MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            StreamMediaPlayer.MediaPlayer.Play();
        }

        /// <summary>
        /// Change playing stream
        /// </summary>
        /// <param name="channel">Twitch channel name</param>
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

            mainPageVM.LoadQualityList(playlist);
            ChangeStream(playlist.Playlist[0]);
        }

        /// <summary>
        /// Change current playing stream
        /// </summary>
        /// <param name="stream">Stream source</param>
        private void ChangeStream(HLSStream stream)
        {
            if (stream is null)
                return;

            StreamMediaPlayer.Source = MediaSource.CreateFromUri(new Uri(stream.Url));
            StreamMediaPlayer.MediaPlayer.Play();
            currentStream = stream;

            StreamMediaPlayer.AreTransportControlsEnabled = true;
        }

        /// <summary>
        /// Stop current playing stream
        /// </summary>
        private void StopStream()
        {
            mainPageVM.StreamQualities.ClearQualityList();

            StreamMediaPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/SplashAssets/SplashVideo.mp4"));
            StreamMediaPlayer.AreTransportControlsEnabled = false;
            StreamMediaPlayer.MediaPlayer.Play();
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
            if (currentStream is null)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ChangeStream(obj.ChannelId);
                });
            }

            var streamer = jadiscoApi.Streamers.SingleOrDefault(x => x.Id == obj.StreamerId);

            var streamerName = (streamer != null) ? streamer.Name : "Unknown";

            var navigationView = new NavigationViewItemViewModel
            {
                Text = $"{streamerName} - {obj.ServiceName}",
                IsEnabled = obj.ServiceName == "twitch",
                Service = obj
            };

            mainPageVM.NavigationViewItems.Add(navigationView);
        }

        private async void JadiscoApi_OnStreamWentOffline(Service obj)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (obj.Equals(currentService))
                {
                    StopStream();
                }
            });

            var navigationView = mainPageVM.NavigationViewItems.SingleOrDefault(x => x.Service.Equals(obj));

            if (navigationView != null)
            {
                mainPageVM.NavigationViewItems.Remove(navigationView);
            }

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
            var vm = sender.SelectedItem as NavigationViewItemViewModel;

            currentService = vm.Service;

            ChangeStream(vm.Service.ChannelId);
        }

        private void ShowFlyout_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void DonateWonziu_Click(object sender, RoutedEventArgs e)
        {
            _ = Launcher.LaunchUriAsync(new Uri("https://streamlabs.com/wonziu"));
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