using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

using Twitch.Api.Models;

namespace Twitch.Api
{
    public static class UsherService
    {
        public static string GetStreamLink(string channel, string sig, string token)
        {
            Random random = new Random();

            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "player", "twitchweb" },
                { "p", random.Next(999999).ToString() },
                { "type", "any" },
                { "allow_source", "true" },
                { "allow_audio_only", "true" },
                { "allow_spectre", "false" },
                { "fast_bread", "true" },
                { "sig", sig },
                { "token", token }
            };

            string paramsString = string.Join("&", parameters.Select(s => $"{s.Key}={HttpUtility.UrlEncode(s.Value)}"));

            return $"https://usher.ttvnw.net/api/channel/hls/{channel}.m3u8?{paramsString}";
        }

        public static HLSPlaylist ParsePlaylists(string url)
        {
            WebRequest request = WebRequest.Create(url);

            if (request != null)
            {
                request.Method = "GET";
                request.Timeout = 12000;
                request.ContentType = "application/json";

                using (WebResponse response = request.GetResponse())
                {
                    return HLSPlaylist.GetFromStream(response.GetResponseStream());
                }
            }

            return null;
        }
    }
}