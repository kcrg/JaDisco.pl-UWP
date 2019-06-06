using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

using IrcDotNet;
using Jadisco.Api;

namespace Jadisco.UWP.ViewModels.Poorchat
{
    public class ChatViewModel : BaseViewModel
    {
        #region Private variables
        DependencyObject _window = null;

        PoorchatIrcClient _poorChatClient = new PoorchatIrcClient();

        int _chatMaxSize = 100;

        private ObservableCollection<ChatMessageViewModel> _messages = new ObservableCollection<ChatMessageViewModel>();

        private ObservableCollection<EmoticonViewModel> _emoticons = new ObservableCollection<EmoticonViewModel>();
        #endregion

        #region Properties
        public ObservableCollection<ChatMessageViewModel> Messages
        {
            get => _messages;
            set { _messages = value; NotifyPropertyChanged(); }
        }

        public ObservableCollection<EmoticonViewModel> Emoticons
        {
            get => _emoticons;
            set { _emoticons = value; NotifyPropertyChanged(); }
        }
        #endregion

        #region Public methods
        public ChatViewModel(DependencyObject window)
        {
            _window = window;

            SetupIrcClient();
            SetupEmoticonsMenu().Wait();
        }

        public async Task AddMessage(ChatMessageViewModel message)
        {
            await _window.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                _messages.Add(message);

                if (_messages.Count > _chatMaxSize)
                {
                    _messages.RemoveAt(0);
                }

                NotifyPropertyChanged("Messages");
            });
        }
        #endregion

        #region Private methods
        #region Emoticons
        private async Task SetupEmoticonsMenu()
        {
            var emoticons = await PoorchatApi.GetEmoticonsAsync();

            foreach (var emoticon in emoticons)
            {
                var emoticonVM = new EmoticonViewModel
                {
                    ImageSource = new BitmapImage(new Uri(emoticon.Url))
                };

                Emoticons.Add(emoticonVM);
            }
        }
        #endregion

        #region IRC Client
        private void SetupIrcClient()
        {
            _poorChatClient.FloodPreventer = new IrcStandardFloodPreventer(4, 2000);
            _poorChatClient.Connected += IrcClient_Connected;
            _poorChatClient.Disconnected += IrcClient_Disconnected;
            _poorChatClient.Registered += IrcClient_Registered;
            _poorChatClient.PoorchatMessage += PoorchatClient_PoorchatMessage;

            _poorChatClient.Connect();
        }

        private async void PoorchatClient_PoorchatMessage(object sender, PoorchatMessage e)
        {
            await _window.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                await AddMessage(new ChatMessageViewModel(e.User, e.UserModes, e.Message));
            });
        }

        private void IrcClient_Connected(object sender, EventArgs e)
        {
            Debug.WriteLine("[Poorchat] Connected!");
        }

        private void IrcClient_Registered(object sender, EventArgs e)
        {
            Debug.WriteLine("[Poorchat] Registered!");
            _poorChatClient.Channels.Join("#jadisco");
        }

        private void IrcClient_Disconnected(object sender, EventArgs e)
        {
            Debug.WriteLine("[Poorchat] Disconnected!");
        }
        #endregion
        #endregion
    }
}
