using System.Collections.Generic;

namespace JaDisco_UWP.ViewModels
{
    public class StreamQualitiesViewModel : BaseViewModel
    {
        public ObservableCollection<StreamQualityViewModel> Qualities { get; } = new ObservableCollection<StreamQualityViewModel>();


        public bool IsNotAvailable
        {
            get => _isNotAvailable;
            set { _isNotAvailable = value; NotifyPropertyChanged(); }
        }

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