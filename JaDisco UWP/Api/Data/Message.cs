using Newtonsoft.Json;

namespace JaDisco_UWP.Api.Data
{
    public class Message
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("data")]
        public MessageData Data { get; set; }
    }
}
