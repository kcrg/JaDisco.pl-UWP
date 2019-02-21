
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
