using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jadisco.UWP.ViewModels
{
    public class NavigationViewItemViewModel : BaseViewModel
    {
        private string content;
        private bool isEnabled = true;
        private string tag;

        public string Content
        {
            get => content;
            set { content = value; NotifyPropertyChanged(); }
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
    }
}
