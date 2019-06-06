using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IrcDotNet;

using Jadisco.Api;

namespace Jadisco.UWP
{
    public class PoorchatMessage
    {
        public IrcUser User { get; set; }

        public string Message { get; set; }

        public IrcChannel Channel { get; set; }

        public PoorchatUserMode[] UserModes { get; set; }
    }

    public class PoorchatIrcClient : StandardIrcClient
    {
        private readonly string IrcUrl = "irc.poorchat.net";

        public event EventHandler<PoorchatMessage> PoorchatMessage;

        public PoorchatIrcClient()
        {
            RawMessageReceived += PoorchatIrcClient_RawMessageReceived;
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

        public IEnumerable<PoorchatUserMode> GetUserModes(IrcChannelUser user)
        {
            foreach (var mode in user.Modes)
            {
                yield return PoorchatApi.GetMode(mode);
            }
        }

        private void PoorchatIrcClient_RawMessageReceived(object sender, IrcRawMessageEventArgs e)
        {
            switch (e.Message.Command)
            {
                case "PRIVMSG":
                {
                    var channelName = e.Message.Parameters[0];
                    var message = e.Message.Parameters[1];

                    var channel = Channels.SingleOrDefault(m => m.Name == channelName);

                    if (channel is null)
                        break;

                    var user = GetUserFromNickName(e.Message.Source.Name);

                    if (user is null)
                        break;

                    var userChannel = channel.GetChannelUser(user);

                    if (userChannel is null)
                        break;

                    var poorChatMsg = new PoorchatMessage
                    {
                        User = user,
                        Message = message,
                        Channel = channel,
                        UserModes = GetUserModes(userChannel).ToArray()
                    };

                    PoorchatMessage?.Invoke(this, poorChatMsg);
                } break;
            }
        }
    }
}
