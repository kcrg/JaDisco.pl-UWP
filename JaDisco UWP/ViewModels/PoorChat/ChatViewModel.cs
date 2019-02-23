using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using IrcDotNet;

namespace JaDisco_UWP.ViewModels.PoorChat
{
    public class ChatViewModel : BaseViewModel
    {
        #region Properties
        private List<ChatMessageViewModel> _messages;

        public List<ChatMessageViewModel> Messages
        {
            get => _messages;
            set { _messages = value; NotifyPropertyChanged(); }
        }
        #endregion

        #region Private variables
        StandardIrcClient ircClient = new StandardIrcClient();
        #endregion

        public ChatViewModel()
        {

            IrcRegistrationInfo irc = new IrcUserRegistrationInfo
            {
                NickName = "Guest",
                UserName = "Guest",
                RealName = "Guest"
            };

            ircClient.FloodPreventer = new IrcStandardFloodPreventer(4, 2000);
            ircClient.Connected += IrcClient_Connected;
            ircClient.Disconnected += IrcClient_Disconnected;
            ircClient.Registered += IrcClient_Registered;

            ircClient.RawMessageReceived += IrcClient_RawMessageReceived;

            using (var registeredEvent = new ManualResetEventSlim(false))
            {
                using (var connectedEvent = new ManualResetEventSlim(false))
                {
                    ircClient.Connected += (sender2, e2) => connectedEvent.Set();
                    ircClient.Registered += (sender2, e2) => registeredEvent.Set();
                    ircClient.Connect("irc.poorchat.net", false, irc);

                    if (!connectedEvent.Wait(10000))
                    {
                        ircClient.Dispose();
                        Debug.WriteLine("Timeout!");
                        return;
                    }
                }

                if (!registeredEvent.Wait(10000))
                {
                    Debug.WriteLine("Could not register");
                    return;
                }
            }

            ircClient.Channels.Join("#jadisco");
        }

        private void IrcClient_RawMessageReceived(object sender, IrcRawMessageEventArgs e)
        {
            Debug.WriteLine($"Raw: {e.Message} {e.RawContent}");
        }

        private void IrcClient_Connected(object sender, EventArgs e)
        {
            Debug.WriteLine("Connected!");
        }

        private void IrcClient_Registered(object sender, EventArgs e)
        {
            var client = (IrcClient)sender;

            client.LocalUser.LeftChannel += LocalUser_LeftChannel;
            client.LocalUser.NoticeReceived += LocalUser_NoticeReceived;
            client.LocalUser.MessageReceived += LocalUser_MessageReceived;
            client.LocalUser.JoinedChannel += LocalUser_JoinedChannel;

            Debug.WriteLine("Registered!");
        }

        private void LocalUser_LeftChannel(object sender, IrcChannelEventArgs e)
        {
            Debug.WriteLine($"Left");
        }

        private void LocalUser_NoticeReceived(object sender, IrcMessageEventArgs e)
        {
            Debug.WriteLine($"Notice: {e.Text}");
        }

        private void LocalUser_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            Debug.WriteLine($"Message: {e.Text}");
        }

        private void LocalUser_JoinedChannel(object sender, IrcChannelEventArgs e)
        {
            Debug.WriteLine($"Joined!");
        }

        private void IrcClient_Disconnected(object sender, EventArgs e)
        {
            Debug.WriteLine("Disconnected!");

        }

        #region Public methods
        public void AddMessage(ChatMessageViewModel message)
        {
            _messages.Add(message);
            NotifyPropertyChanged("Messages");
        }
        #endregion
    }
}
