using System.Collections.Generic;

namespace JaDisco_UWP.ViewModels
{
    public class StreamQualitiesViewModel : BaseViewModel
    {
        private bool _isNotAvailable = true;

        public List<StreamQualityViewModel> Qualities { get; } = new List<StreamQualityViewModel>();

        public bool IsNotAvailable
        {
            get => _isNotAvailable;
            set { _isNotAvailable = value; NotifyPropertyChanged(); }
        }

        public void ClearQualityList()
        {
            Qualities.Clear();
            NotifyPropertyChanged("Qualities");
        }

        public void AddQuality(StreamQualityViewModel quality)
        {
            Qualities.Add(quality);
            NotifyPropertyChanged("Qualities");
        }
    }
}