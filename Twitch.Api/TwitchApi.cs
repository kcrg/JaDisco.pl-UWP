using Newtonsoft.Json;
using System.Collections.Generic;
using Twitch.Api.Models;

namespace Twitch.Api
{
    public static class TwitchApi
    {
        private const string TwitchClientId = "ot11j3ucjrcx1tid3f6yfyhoi2za4g";

        public static AccessToken GetAccessToken(string channel)
        {
            Dictionary<string, string> header = new Dictionary<string, string>
            {
                { "Accept", "application/vnd.twitchtv.v5+json" },
                { "Client-ID", TwitchClientId }
            };

            string result = ApiRequestHelper.ExecuteWebRequest($"https://api.twitch.tv/api/channels/{channel}/access_token.json", header);

            return result != null ? JsonConvert.DeserializeObject<AccessToken>(result) : null;
        }
    }
}
