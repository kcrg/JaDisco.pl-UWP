using System;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace JaDisco_UWP.Views
{
    internal partial class ExtendedSplash : Page
    {
        private readonly SplashScreen splash; // Variable to hold the splash screen object.
        internal bool dismissed = false; // Variable to track splash screen dismissal status.
        internal Frame rootFrame;

        public ExtendedSplash(SplashScreen splashscreen, bool loadState)
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;

            splash = splashscreen;

            if (splash != null)
            {
                // Register an event handler to be executed when the splash screen has been dismissed.
                splash.Dismissed += new TypedEventHandler<SplashScreen, object>(DismissedEventHandler);
            }
        }

        // Include code to be executed when the system has transitioned from the splash screen to the extended splash screen (application's first view).
        private void DismissedEventHandler(SplashScreen sender, object e)
        {
            dismissed = true;

            // Complete app setup operations here...
            //extendedSplashVideo.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Wide310x150Logo.scale-400.png"));
            DismissExtendedSplash();
        }

        private async void DismissExtendedSplash()
        {
            //await Task.Delay(TimeSpan.FromSeconds(5));

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                rootFrame = new Frame
                {
                    Content = new MainPage()
                };
                Window.Current.Content = rootFrame;
            });
        }
    }
}