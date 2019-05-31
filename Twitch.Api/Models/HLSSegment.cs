using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twitch.Api
{
    public class HLSSegment
    {
        public float Duration { get; set; }

        public string Url { get; set; }

        public bool Prefetch { get; set; } = false;

        public override int GetHashCode()
        {
            int hash = 19;
            hash = hash * 31 + Url.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            return GetHashCode() == obj.GetHashCode();
        }
    }
}
