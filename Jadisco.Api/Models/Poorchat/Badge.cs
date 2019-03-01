using Newtonsoft.Json;

namespace Jadisco.Api.Models.Poorchat
{
    public class Badge
    {
        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }

        [JsonIgnore]
        public string Url { get; set; }
    }
}
