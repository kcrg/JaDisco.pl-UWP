using System;
using System.Collections.ObjectModel;
using Windows.System;

namespace Jadisco.UWP.ViewModels
{
    internal class MainPageViewModel : BaseViewModel
    {
        #region Private members
        private ObservableCollection<NavigationViewItemViewModel> navigationViewItems = new ObservableCollection<NavigationViewItemViewModel>();

        #endregion

        #region Public properties
        public ObservableCollection<NavigationViewItemViewModel> NavigationViewItems
        {
            get { return navigationViewItems; }
            set { navigationViewItems = value; NotifyPropertyChanged(); }
        }

        public StreamQualitiesViewModel StreamQualities { get; set; } = new StreamQualitiesViewModel();
        #endregion

        public MainPageViewModel()
        {
            navigationViewItems.Add(new NavigationViewItemViewModel { Content = "Wonziu - Youtube", Tag = "wow" });
            navigationViewItems.Add(new NavigationViewItemViewModel { Content = "Wonziu - Twitch", Tag = "wow" });
        }


    }
}