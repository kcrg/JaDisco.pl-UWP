using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;

using Newtonsoft.Json;
using Twitch.Api.Models;

namespace Twitch.Api
{
    public static class TwitchApi
    {
        readonly static string TwitchClientId = "ot11j3ucjrcx1tid3f6yfyhoi2za4g";

        public static AccessToken GetAccessToken(string channel)
        {
            var header = new Dictionary<string, string>
            {
                { "Accept", "application/vnd.twitchtv.v5+json" },
                { "Client-ID", TwitchClientId }
            };

            var result = ApiRequestHelper.ExecuteWebRequest($"https://api.twitch.tv/api/channels/{channel}/access_token.json", header);

            if (result != null)
            {
                return JsonConvert.DeserializeObject<AccessToken>(result);
            }

            return null;
        }
    }
}
