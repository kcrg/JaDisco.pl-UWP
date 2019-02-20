using Newtonsoft.Json;

namespace Twitch.Api.Data
{
    public class AccessToken
    {
        [JsonProperty("token")]
        public Token Token { get; set; }

        [JsonProperty("sig")]
        public string Sig { get; set; }

        [JsonProperty("mobile_restricted")]
        public bool MobileRestricted { get; set; }
    }
}
