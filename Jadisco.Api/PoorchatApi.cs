using Jadisco.Api.Models.Poorchat;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jadisco.Api
{
    public static class PoorchatApi
    {
        #region Private members
        static List<Emoticon> emoticons;

        static List<Badge> badges;

        static ChannelBadge channelBadge;
        #endregion

        #region Public methods
        static public PoorchatUserMode GetMode(char mode)
        {
            switch (mode)
            {
                case 'q':
                    return PoorchatUserMode.Owner;
                case 'a':
                    return PoorchatUserMode.Admin;
                case 'o':
                    return PoorchatUserMode.Moderator;
                case 'v':
                    return PoorchatUserMode.Subscriber;
                case 'h':
                    return PoorchatUserMode.VIP;
                default:
                    return PoorchatUserMode.None;
            }
        }

        static public async Task<Emoticon> GetEmoticonAsync(string name)
        {
            if (emoticons is null)
            {
                emoticons = (await DownloadDataAsync<Emoticon[]>("https://api.poorchat.net/v1/channels/jadisco/emoticons")).ToList();

                foreach (var emoticon in emoticons)
                {
                    emoticon.Url = $"https://static.poorchat.net/emoticons/{emoticon.File}/1x";
                }
            }

            return emoticons?.SingleOrDefault(m => m.Name == name);
        }

        static public async Task<Badge> GetBadgeAsync(string mode)
        {
            if (badges is null)
            {
                badges = (await DownloadDataAsync<Badge[]>("https://api.poorchat.net/v1/badges")).ToList();

                foreach (var badge in badges)
                {
                    badge.Url = $"https://static.poorchat.net/badges/{badge.File}/1x";
                }
            }

            return badges?.SingleOrDefault(m => m.Mode == mode);
        }

        static public async Task<Badge> GetBadgeAsync(PoorchatUserMode mode)
        {
            return await GetBadgeAsync(((char)mode).ToString());
        }

        static public async Task<SubscriberBadge> GetSubscriberBadgeAsync(int month, string channelName = "jadisco")
        {
            if (channelBadge is null)
            {
                channelBadge = await DownloadDataAsync<ChannelBadge>($"https://api.poorchat.net/v1/channels/{channelName}/badges");

                foreach (var subscriber in channelBadge.Subscriber)
                {
                    subscriber.Url = $"https://static.poorchat.net/badges/{subscriber.File}/1x";
                }
            }

            return channelBadge.Subscriber.SingleOrDefault(m => m.Months == month);
        }
        #endregion

        #region Helpers
        static private async Task<T> DownloadDataAsync<T>(string url)
        {
            var text = await Shared.ApiRequestHelper.ExecuteWebRequestAsync(url);

            var data = JsonConvert.DeserializeObject<T>(text);

            return data;
        }
        #endregion

    }
}
