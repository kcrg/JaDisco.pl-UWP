using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaDisco_UWP.ViewModels.PoorChat
{
    public class ChatViewModel : BaseViewModel
    {
        private List<ChatMessageViewModel> _messages;

        public List<ChatMessageViewModel> Messages
        {
            get => _messages;
            set { _messages = value; NotifyPropertyChanged(); }
        }

        public void AddMessage(ChatMessageViewModel message)
        {
            _messages.Add(message);
            NotifyPropertyChanged("Messages");
        }
    }
}
