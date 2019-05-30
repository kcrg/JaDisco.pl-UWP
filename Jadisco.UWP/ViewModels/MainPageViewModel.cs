using Jadisco.UWP.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Twitch.Api.Models;
using Windows.System;

namespace Jadisco.UWP.ViewModels
{
    internal class MainPageViewModel : BaseViewModel
    {
        #region Private members
        private ObservableCollection<NavigationViewItemViewModel> navigationViewItems = new ObservableCollection<NavigationViewItemViewModel>();

        private readonly MainPage mainPage;
        #endregion

        #region Public properties
        public ObservableCollection<NavigationViewItemViewModel> NavigationViewItems
        {
            get { return navigationViewItems; }
            set { navigationViewItems = value; NotifyPropertyChanged(); }
        }

        public StreamQualitiesViewModel StreamQualities { get; set; } = new StreamQualitiesViewModel();
        #endregion

        public MainPageViewModel(MainPage mainPage)
        {
            this.mainPage = mainPage;
        }

        #region Public methods
        /// <summary>
        /// Load list of avaliable qualities for stream
        /// </summary>
        /// <param name="playlist">Stream playlist source</param>
        public void LoadQualityList(HLSPlaylist playlist)
        {
            if (playlist?.Playlist != null && playlist.Playlist.Count() > 0)
            {
                StreamQualities.ClearQualityList();

                foreach (var stream in playlist.Playlist)
                {
                    if (stream.Name.StartsWith("audio"))
                        continue;

                    StreamQualities.AddQuality(new StreamQualityViewModel
                    {
                        Name = stream.Name,
                        HLSStream = stream
                    });
                }
            }
        }
        #endregion
    }
}