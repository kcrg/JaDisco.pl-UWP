using System;
using System.Collections.ObjectModel;
using Windows.System;

namespace JaDisco_UWP.ViewModels
{
    internal class MainPageViewModel : BaseViewModel
    {
        #region Private members
        private ObservableCollection<NavigationViewItemViewModel> navigationViewItems = new ObservableCollection<NavigationViewItemViewModel>();
        #endregion

        public ObservableCollection<NavigationViewItemViewModel> NavigationViewItems
        {
            get { return navigationViewItems; }
            set { navigationViewItems = value; NotifyPropertyChanged(); }
        }

        public StreamQualitiesViewModel StreamQualities { get; set; } = new StreamQualitiesViewModel();

        public MainPageViewModel()
        {
            navigationViewItems.Add(new NavigationViewItemViewModel { Content = "Wonziu - Youtube", Tag = "wow" });
            navigationViewItems.Add(new NavigationViewItemViewModel { Content = "Wonziu - Twitch", Tag = "wow" });
        }
    }
}