using Newtonsoft.Json;

namespace JaDisco_UWP.Api.Data
{
    public class Streamer
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
