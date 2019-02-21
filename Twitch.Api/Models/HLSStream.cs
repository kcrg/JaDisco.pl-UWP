namespace Twitch.Api.Models
{
    public class HLSStream
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public long Bitrate { get; set; }
    }
}