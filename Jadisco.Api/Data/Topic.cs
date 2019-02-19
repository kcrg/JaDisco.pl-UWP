using System;
using Newtonsoft.Json;

namespace Jadisco.Api.Data
{
    public class Topic
    {
        [JsonProperty("id")]
        public double Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
