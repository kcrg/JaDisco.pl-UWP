using System.Collections.Generic;
using System.Threading.Tasks;

using Twitch.Api.Models;

using Newtonsoft.Json;

using Shared;

namespace Twitch.Api
{
    public static class TwitchApi
    {
        private static readonly string TwitchClientId = "ot11j3ucjrcx1tid3f6yfyhoi2za4g";

        public static async Task<AccessToken> GetAccessTokenAsync(string channel)
        {
            Dictionary<string, string> header = new Dictionary<string, string>
            {
                { "Accept", "application/vnd.twitchtv.v5+json" },
                { "Client-ID", TwitchClientId }
            };

            string result = await ApiRequestHelper.ExecuteWebRequestAsync($"https://api.twitch.tv/api/channels/{channel}/access_token.json", header);

            if (result != null)
            {
                return JsonConvert.DeserializeObject<AccessToken>(result);
            }

            return null;
        }
    }
}
