using Newtonsoft.Json;

namespace Jadisco.Api.Models.Notifier
{
    public class ApiMessage
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("data")]
        public MessageData Data { get; set; }
    }
}