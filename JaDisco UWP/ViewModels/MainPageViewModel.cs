using System;
using Windows.System;

namespace JaDisco_UWP.ViewModels
{
    internal class MainPageViewModel
    {
        //public void MemoryCleanup()
        //{
        //    GC.Collect();
        //}

        public async void LaunchUri(string uri)
        {
            _ = await Launcher.LaunchUriAsync(new Uri(uri));
        }
    }
}