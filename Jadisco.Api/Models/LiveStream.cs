using Jadisco.Api.Models.Notifier;
using Newtonsoft.Json;
using System;

namespace Jadisco.Api.Models
{
    public class LiveStream
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