using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Jadisco.UWP
{
    public sealed partial class App : Application
    {
        public static bool RunningOnDesktop => AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop";
        public static bool RunningOnXbox => AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox";
        public static bool RunningOnMobile => AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile";
        public static bool RunningWithDarkTheme => Current.RequestedTheme == ApplicationTheme.Dark;

        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (!(Window.Current.Content is Frame rootFrame))
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // Titlebar customization
                if (RunningOnDesktop)
                {
                    CoreApplicationViewTitleBar CoreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                    CoreTitleBar.ExtendViewIntoTitleBar = true;

                    ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
                    titleBar.ButtonBackgroundColor = Colors.Transparent;
                    titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

                    if (RunningWithDarkTheme)
                    {
                        titleBar.ButtonForegroundColor = Colors.White;
                        titleBar.ButtonHoverForegroundColor = Colors.White;
                        titleBar.ButtonHoverBackgroundColor = Colors.Black;
                        titleBar.BackgroundColor = Colors.Black;
                    }
                    else
                    {
                        titleBar.ButtonForegroundColor = Colors.Black;
                        titleBar.ButtonHoverForegroundColor = Colors.Black;
                        titleBar.ButtonHoverBackgroundColor = Colors.White;
                        titleBar.BackgroundColor = Colors.White;
                    }
                }

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}