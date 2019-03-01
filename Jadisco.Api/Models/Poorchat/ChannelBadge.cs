using Newtonsoft.Json;

namespace Jadisco.Api.Models.Poorchat
{
    public class SubscriberBadge
    {
        [JsonProperty("months")]
        public long Months { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }

        [JsonIgnore]
        public string Url { get; set; }
    }

    public class ChannelBadge
    {
        [JsonProperty("subscriber")]
        public SubscriberBadge[] Subscriber { get; set; }
    }
}
