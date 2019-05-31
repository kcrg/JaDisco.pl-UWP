using Jadisco.UWP.Views.CustomDialogs;
using Jadisco.UWP.ViewModels;

using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.ApplicationModel.Core;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;

using Twitch.Api;
using Twitch.Api.Models;
using Jadisco.Api;
using Jadisco.Api.Models;

using muxc = Microsoft.UI.Xaml.Controls;
using toolkit = Microsoft.Toolkit.Uwp.UI.Controls;
using Jadisco.UWP.Views.Controls;

namespace Jadisco.UWP.Views
{
    public sealed partial class MainPage : Page
    {
        private bool LeftChat, HiddenChat = false;
        private readonly Uri ChatUri = new Uri("https://client.poorchat.net/jadisco");
        private readonly Uri BlankUri = new Uri("about:blank");

        private readonly ToolTip chatHideToolTip = new ToolTip();

        private readonly JadiscoApi jadiscoApi = new JadiscoApi();
        private HLSStream currentHLSStream = null;
        private Service currentService = null;

        private readonly MainPageViewModel mainPageVM;

        private NavigationViewItemViewModel noStreamLabel;

        public MainPage()
        {
            InitializeComponent();

            mainPageVM = new MainPageViewModel(this);
            DataContext = mainPageVM;

            AddNoStreamLabel();

            jadiscoApi.OnTopicChanged += JadiscoApi_OnTopicChanged;
            jadiscoApi.OnStreamWentOnline += JadiscoApi_OnStreamWentOnline;
            jadiscoApi.OnStreamWentOffline += JadiscoApi_OnStreamWentOffline;
            jadiscoApi.Connect();

            Window.Current.Activated += Window_Activated;
        }

        #region Jadisco.Api events
        private async void JadiscoApi_OnStreamWentOnline(Service obj)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (currentHLSStream is null)
                {
                    currentService = obj;
                    await PlayStream(obj);
                }

                var navigationView = mainPageVM.NavigationViewItems.SingleOrDefault(x => {
                    if (x.Service == null)
                        return false;

                    return x.Service.Equals(obj);
                });

                if (navigationView is null)
                {
                    var streamer = jadiscoApi.Streamers.SingleOrDefault(x => x.Id == obj.StreamerId);

                    var streamerName = (streamer != null) ? streamer.Name : "Unknown";

                    navigationView = new NavigationViewItemViewModel
                    {
                        Text = $"{streamerName} - {char.ToUpper(obj.ServiceName[0])}{obj.ServiceName.Substring(1)}/{obj.ChannelId}",
                        IsEnabled = obj.ServiceName == "twitch",
                        Service = obj,
                        ToolTip = obj.ServiceName != "twitch" ? "Nie obsługiwane" : string.Empty
                    };

                    mainPageVM.NavigationViewItems.Add(navigationView);

                    noStreamLabel.Visibility = Visibility.Collapsed;
                }
            });
        }

        private async void JadiscoApi_OnStreamWentOffline(Service obj)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // check if current playing stream has finished
                if (currentService != null && obj.Equals(currentService))
                {
                    currentService = null;
                    StopStream();
                    StreamPlayer.PlaySplashScreen();
                }

                var navigationView = mainPageVM.NavigationViewItems.SingleOrDefault(x => {
                    if (x.Service == null)
                        return false;

                    return x.Service.Equals(obj);
                });

                if (navigationView != null)
                {
                    mainPageVM.NavigationViewItems.Remove(navigationView);
                }

                if (mainPageVM.NavigationViewItems.Count == 1)
                {
                    noStreamLabel.Visibility = Visibility.Visible;
                }
            });

            currentHLSStream = null;
        }

        private async void JadiscoApi_OnTopicChanged(Topic topic)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                StatusTextBlock.Text = topic.Text.Trim().Replace("\n\n", " ").Replace("\n", " ");

                StatusFlyoutTextBlock.Text = topic.Text.Trim();
                StatusDateFlyoutTextBlock.Text = "Dodane: " + topic.UpdatedAt.ToString().Replace(" +00:00", string.Empty);
            });
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Play specified service
        /// </summary>
        /// <param name="service">Service to play</param>
        private async Task PlayStream(Service service)
        {
            if (StreamPlayer.SupportedServices.SingleOrDefault(x => x == service.ServiceName) is null)
                return;

            StopStream();

            try
            {
                if (StreamPlayer.Setup(service))
                {
                    mainPageVM.LoadQualityList(StreamPlayer.HLSPlaylist);
                    StreamPlayer.PlayStream();
                }
            }
            catch (Exception ex)
            {
                ErrorDialog errorDialog = new ErrorDialog($"Nie udało się odtworzyć strumienia.\nPowód: {ex.Message}", ErrorDialog.Type.Error);
                await errorDialog.ShowAsync();
            }
        }

        /// <summary>
        /// Stop current playing stream
        /// </summary>
        private void StopStream()
        {
            mainPageVM?.StreamQualities?.ClearQualityList();

            StreamPlayer.StopStream();
        }

        private void AddNoStreamLabel()
        {
            noStreamLabel = new NavigationViewItemViewModel
            {
                Text = $"Brak streamów",
                IsEnabled = false,
                Service = null,
                ToolTip = string.Empty,
            };

            mainPageVM.NavigationViewItems.Add(noStreamLabel);
        }

        private void HideChat(bool fromHideButton)
        {
            if (!HiddenChat)
            {
                chatHideToolTip.Content = "Pokaż czat";
                ChatHideButton.Icon = new SymbolIcon(Symbol.Message);
                ToolTipService.SetToolTip(ChatHideButton, chatHideToolTip);

                if (!LeftChat)
                {
                    RightColumn.Width = new GridLength(0);
                }
                else if (LeftChat)
                {
                    LeftColumn.Width = new GridLength(0);
                }

                ChatGrid.Visibility = Visibility.Collapsed;
                ChatPositionButton.IsEnabled = false;
                ChatWebView.Navigate(BlankUri);
                HiddenChat = true;
            }
            else if (HiddenChat)
            {
                chatHideToolTip.Content = "Schowaj czat";
                ChatHideButton.Icon = new SymbolIcon(Symbol.LeaveChat);
                ToolTipService.SetToolTip(ChatHideButton, chatHideToolTip);

                if (fromHideButton)
                {
                    if (!LeftChat)
                    {
                        RightColumn.Width = new GridLength(2300, GridUnitType.Star);
                    }
                    else if (LeftChat)
                    {
                        LeftColumn.Width = new GridLength(2300, GridUnitType.Star);
                    }

                    ChatGrid.Visibility = Visibility.Visible;
                    ChatPositionButton.IsEnabled = true;
                    ChatWebView.Navigate(ChatUri);

                    HiddenChat = false;
                }
            }
        }
        #endregion

        #region Application events
        private void StreamQualityButton_Checked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            var data = radio.DataContext as StreamQualityViewModel;

            if (data is null)
                return;

            StreamPlayer.StopStream();
            StreamPlayer.PlayHLSStream(data.HLSStream);
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

            if (currentHLSStream != null)
            {
                StreamPlayer.RestartStream();
            }
        }

        private async void NavView_SelectionChanged(muxc.NavigationView sender, muxc.NavigationViewSelectionChangedEventArgs args)
        {
            var vm = sender.SelectedItem as NavigationViewItemViewModel;

            currentService = vm.Service;

            await PlayStream(vm.Service);
        }

        private void ShowFlyout_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void DonateWonziu_Click(object sender, RoutedEventArgs e)
        {
            _ = Launcher.LaunchUriAsync(new Uri("https://streamlabs.com/wonziu"));
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
            {
                StatusTextBlock.Foreground = new SolidColorBrush(Colors.Gray);
                AppLogo.Source = new BitmapImage(new Uri("ms-appx:///Assets/TitleBarAssets/JaDiscoStaticLogoGray.png", UriKind.Absolute));
            }
            else
            {
                if (App.RunningWithDarkTheme)
                {
                    StatusTextBlock.Foreground = new SolidColorBrush(Colors.White);
                    AppLogo.Source = new BitmapImage(new Uri("ms-appx:///Assets/TitleBarAssets/JaDiscoStaticLogoLight.png", UriKind.Absolute));
                }
                else
                {
                    StatusTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                    AppLogo.Source = new BitmapImage(new Uri("ms-appx:///Assets/TitleBarAssets/JaDiscoStaticLogoDark.png", UriKind.Absolute));
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.RunningOnXbox || App.RunningOnMobile)
            {
                ChatInNewWindow.Visibility = Visibility.Collapsed;
                StatusTextBlock.Margin = new Thickness(90, 0, 0, 0);
            }

            if (App.RunningWithDarkTheme)
            {
                AppLogo.Source = new BitmapImage(new Uri("ms-appx:///Assets/TitleBarAssets/JaDiscoStaticLogoLight.png", UriKind.Absolute));
            }
            else
            {
                AppLogo.Source = new BitmapImage(new Uri("ms-appx:///Assets/TitleBarAssets/JaDiscoStaticLogoDark.png", UriKind.Absolute));
            }
        }

        private async void StatusFlyoutTextBlock_LinkClicked(object sender, toolkit.LinkClickedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(e.Link));
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (StreamPlayer is null)
                return;

            var rb = sender as RadioButton;

            if (rb is null)
                return;

            switch (rb.Tag.ToString())
            {
                case "NativeLowLatency":
                    StreamPlayer.PlayerType = StreamPlayerType.NativeLowLatency;
                    break;
                case "NativeOld":
                    StreamPlayer.PlayerType = StreamPlayerType.NativeOld;
                    break;
                case "Web":
                    StreamPlayer.PlayerType = StreamPlayerType.Web;
                    break;
            }

            StreamPlayer.StopStream();
            StreamPlayer.PlayStream();
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
            ChatHideButton.Icon = new SymbolIcon(Symbol.Message);
        }
        #endregion
    }
}