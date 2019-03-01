using System;

using Newtonsoft.Json;

namespace Jadisco.Api.Models.Poorchat
{
    public class Emoticon
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }

        [JsonProperty("subscribers_only")]
        public bool SubscribersOnly { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonIgnore]
        public string Url { get; set; }
    }
}
