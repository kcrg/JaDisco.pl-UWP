using System;
using Newtonsoft.Json;

namespace JaDisco_UWP.Api.Data
{
    public class Stream
    {
        [JsonProperty("status")]
        public bool Status { get; set; }

        [JsonProperty("viewers")]
        public long Viewers { get; set; }

        [JsonProperty("services")]
        public Service[] Services { get; set; }

        [JsonProperty("online_at")]
        public DateTimeOffset OnlineAt { get; set; }

        [JsonProperty("offline_at")]
        public DateTimeOffset OfflineAt { get; set; }
    }
}
