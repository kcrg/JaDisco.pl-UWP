using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IrcDotNet;

namespace JaDisco_UWP
{
    public class PoorChatMessage
    {
        public string Author { get; set; }

        public string Message { get; set; }

        public string Channel { get; set; }
    }

    public class PoorChatIrcClient : StandardIrcClient
    {
        private readonly string IrcUrl = "irc.poorchat.net";

        public event EventHandler<PoorChatMessage> PoorChatMessage;

        public PoorChatIrcClient()
        {
            RawMessageReceived += PoorChatIrcClient_RawMessageReceived;
        }

        public void Connect()
        {
            IrcRegistrationInfo irc = new IrcUserRegistrationInfo
            {
                NickName = "Guest",
                UserName = "Guest",
                RealName = "Guest"
            };

            Connect(IrcUrl, false, irc);
        }

        private void PoorChatIrcClient_RawMessageReceived(object sender, IrcRawMessageEventArgs e)
        {
            switch (e.Message.Command)
            {
                case "PRIVMSG":
                {
                    var channelName = e.Message.Parameters[0];
                    var text = e.Message.Parameters[1];

                    var channel = Channels.SingleOrDefault(m => m.Name == channelName);

                    if (channel != null)
                    {
                        var poorChatMsg = new PoorChatMessage
                        {
                            Author = e.Message.Source.Name,
                            Message = text,
                            Channel = channel.Name
                        };

                        PoorChatMessage?.Invoke(this, poorChatMsg);
                    }
                } break;
            }
        }
    }
}
