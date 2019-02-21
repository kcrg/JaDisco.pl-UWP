using Newtonsoft.Json;

namespace Twitch.Api.Data
{
    public class Authorization
    {
        [JsonProperty("forbidden")]
        public bool Forbidden { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }

    public class Chansub
    {
        [JsonProperty("restricted_bitrates")]
        public object[] RestrictedBitrates { get; set; }

        [JsonProperty("view_until")]
        public long ViewUntil { get; set; }
    }

    public class Private
    {
        [JsonProperty("allowed_to_view")]
        public bool AllowedToView { get; set; }
    }

    public class Token
    {
        [JsonProperty("adblock")]
        public bool Adblock { get; set; }

        [JsonProperty("authorization")]
        public Authorization Authorization { get; set; }

        [JsonProperty("blackout_enabled")]
        public bool BlackoutEnabled { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("channel_id")]
        public long ChannelId { get; set; }

        [JsonProperty("chansub")]
        public Chansub Chansub { get; set; }

        [JsonProperty("ci_gb")]
        public bool CiGb { get; set; }

        [JsonProperty("geoblock_reason")]
        public string GeoblockReason { get; set; }

        [JsonProperty("device_id")]
        public object DeviceId { get; set; }

        [JsonProperty("expires")]
        public long Expires { get; set; }

        [JsonProperty("game")]
        public string Game { get; set; }

        [JsonProperty("hide_ads")]
        public bool HideAds { get; set; }

        [JsonProperty("https_required")]
        public bool HttpsRequired { get; set; }

        [JsonProperty("mature")]
        public bool Mature { get; set; }

        [JsonProperty("partner")]
        public bool Partner { get; set; }

        [JsonProperty("platform")]
        public object Platform { get; set; }

        [JsonProperty("player_type")]
        public object PlayerType { get; set; }

        [JsonProperty("private")]
        public Private Private { get; set; }

        [JsonProperty("privileged")]
        public bool Privileged { get; set; }

        [JsonProperty("server_ads")]
        public bool ServerAds { get; set; }

        [JsonProperty("show_ads")]
        public bool ShowAds { get; set; }

        [JsonProperty("subscriber")]
        public bool Subscriber { get; set; }

        [JsonProperty("turbo")]
        public bool Turbo { get; set; }

        [JsonProperty("user_id")]
        public object UserId { get; set; }

        [JsonProperty("user_ip")]
        public string UserIp { get; set; }

        [JsonProperty("version")]
        public long Version { get; set; }
    }
}