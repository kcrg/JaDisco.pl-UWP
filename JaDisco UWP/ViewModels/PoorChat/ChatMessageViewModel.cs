using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaDisco_UWP.ViewModels.PoorChat
{
    public class ChatMessageViewModel : BaseViewModel
    {
        private string _author;
        private string _message;

        public string Author
        {
            get => _author;
            set { _author = value; NotifyPropertyChanged(); }
        }

        public string Message
        {
            get => _message;
            set { _message = value; NotifyPropertyChanged(); }
        }
    }
}
