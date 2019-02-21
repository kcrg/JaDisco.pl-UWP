using Newtonsoft.Json;

namespace Jadisco.Api.Models
{
    public class MessageData
    {
        [JsonProperty("streamers")]
        public Streamer[] Streamers { get; set; }

        [JsonProperty("stream")]
        public Stream Stream { get; set; }

        [JsonProperty("topic")]
        public Topic Topic { get; set; }
    }
}