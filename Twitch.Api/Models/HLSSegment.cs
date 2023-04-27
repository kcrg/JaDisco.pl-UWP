namespace Twitch.Api.Models
{
    public class HLSSegment
    {
        public float Duration { get; set; }

        public string Url { get; set; }

        public bool Prefetch { get; set; } = false;

        public override int GetHashCode()
        {
            int hash = 19;
            hash = (hash * 31) + Url.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            return obj != null && GetHashCode() == obj.GetHashCode();
        }
    }
}
