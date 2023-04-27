using Newtonsoft.Json;

namespace Jadisco.Api.Models
{
    public class Service
    {
        [JsonProperty("streamer_id")]
        public long StreamerId { get; set; }

        [JsonProperty("name")]
        public string ServiceName { get; set; }

        [JsonProperty("id")]
        public string ChannelId { get; set; }

        [JsonProperty("status")]
        public bool Status { get; set; }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            int hash = 13;

            hash = (hash * 7) + StreamerId.GetHashCode();
            hash = (hash * 7) + ServiceName.GetHashCode();
            hash = (hash * 7) + ChannelId.GetHashCode();

            return hash;
        }
    }
}