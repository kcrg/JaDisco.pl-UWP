using Newtonsoft.Json;

namespace Jadisco.Api.Models
{
    public class Streamer
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}