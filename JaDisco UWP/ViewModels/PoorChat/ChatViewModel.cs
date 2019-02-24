using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using IrcDotNet;
using Windows.UI.Xaml;

namespace JaDisco_UWP.ViewModels.PoorChat
{
    public class ChatViewModel : BaseViewModel
    {
        #region Properties
        DependencyObject _window = null;

        private ObservableCollection<ChatMessageViewModel> _messages = new ObservableCollection<ChatMessageViewModel>();

        public ObservableCollection<ChatMessageViewModel> Messages
        {
            get => _messages;
            set { _messages = value; NotifyPropertyChanged(); }
        }
        #endregion

        #region Private variables
        PoorChatIrcClient poorChatClient = new PoorChatIrcClient();
        #endregion

        public ChatViewModel(DependencyObject window)
        {
            _window = window;

            poorChatClient.FloodPreventer = new IrcStandardFloodPreventer(4, 2000);
            poorChatClient.Connected += IrcClient_Connected;
            poorChatClient.Disconnected += IrcClient_Disconnected;
            poorChatClient.Registered += IrcClient_Registered;
            poorChatClient.PoorCharMessage += PoorChatClient_PoorCharMessage;

            poorChatClient.Connect();
        }

        private async void PoorChatClient_PoorCharMessage(object sender, PoorChatMessage e)
        {
            var chatMessageVM = new ChatMessageViewModel
            {
                Author = e.Author,
                Message = e.Message
            };

            await AddMessage(chatMessageVM);
        }

        private void IrcClient_Connected(object sender, EventArgs e)
        {
            Debug.WriteLine("[PoorChat] Connected!");
        }

        private void IrcClient_Registered(object sender, EventArgs e)
        {
            Debug.WriteLine("[PoorChat] Registered!");
            poorChatClient.Channels.Join("#jadisco");
        }

        private void IrcClient_Disconnected(object sender, EventArgs e)
        {
            Debug.WriteLine("[PoorChat] Disconnected!");
        }

        #region Public methods
        public async Task AddMessage(ChatMessageViewModel message)
        {
            await _window.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                _messages.Add(message);
                NotifyPropertyChanged("Messages");
            });
        }
        #endregion
    }
}
