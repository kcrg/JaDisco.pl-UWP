using Windows.UI.Xaml;

namespace Jadisco.UWP.ViewModels
{
    public class NavigationViewItemViewModel : BaseViewModel
    {
        private string text;
        private bool isEnabled = true;
        private string tag;
        private string toolTip;
        private Visibility visibility = Visibility.Visible;

        public string Text
        {
            get => text;
            set { text = value; NotifyPropertyChanged(); }
        }

        public bool IsEnabled
        {
            get => isEnabled;
            set { isEnabled = value; NotifyPropertyChanged(); }
        }

        public string Tag
        {
            get => tag;
            set { tag = value; NotifyPropertyChanged(); }
        }

        public string ToolTip
        {
            get => toolTip;
            set { toolTip = value; NotifyPropertyChanged(); }
        }

        public Visibility Visibility
        {
            get => visibility;
            set { visibility = value; NotifyPropertyChanged(); }
        }

        public Jadisco.Api.Models.Service Service { get; set; }
    }
}
