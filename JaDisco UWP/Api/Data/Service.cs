using Newtonsoft.Json;

namespace JaDisco_UWP.Api.Data
{
    public class Service
    {
        [JsonProperty("streamer_id")]
        public long StreamerId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public bool Status { get; set; }
    }
}
