using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace JaDisco_UWP.ViewModels
{
    public class StreamQualitiesViewModel : BaseViewModel
    {
        public ObservableCollection<StreamQualityViewModel> Qualities { get; } = new ObservableCollection<StreamQualityViewModel>();

        public void ClearQualityList()
        {
            Qualities.Clear();
            NotifyPropertyChanged(nameof(Qualities));
        }

        public void AddQuality(StreamQualityViewModel quality)
        {
            Qualities.Add(quality);
            NotifyPropertyChanged(nameof(Qualities));
        }
    }
}