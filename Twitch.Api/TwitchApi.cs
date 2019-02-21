using System.Collections.Generic;

using Twitch.Api.Models;

using Newtonsoft.Json;

namespace Twitch.Api
{
    public static class TwitchApi
    {
        private static readonly string TwitchClientId = "ot11j3ucjrcx1tid3f6yfyhoi2za4g";

        public static AccessToken GetAccessToken(string channel)
        {
            Dictionary<string, string> header = new Dictionary<string, string>
            {
                { "Accept", "application/vnd.twitchtv.v5+json" },
                { "Client-ID", TwitchClientId }
            };

            string result = ApiRequestHelper.ExecuteWebRequest($"https://api.twitch.tv/api/channels/{channel}/access_token.json", header);

            if (result != null)
            {
                return JsonConvert.DeserializeObject<AccessToken>(result);
            }

            return null;
        }
    }
}
