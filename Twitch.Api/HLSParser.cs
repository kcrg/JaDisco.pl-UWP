using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Twitch.Api.Models;

namespace Twitch.Api
{
    public static class HLSParser
    {
        public static HLSPlaylist GetFromStream(Stream stream)
        {
            var playlist = new List<HLSStream>();

            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (!line.StartsWith("#EXT-X-MEDIA:TYPE=VIDEO"))
                        continue;

                    var hls = new HLSStream();

                    hls.Name = GetName(line);

                    line = reader.ReadLine();

                    if (!line.StartsWith("#EXT-X-STREAM-INF"))
                        continue;

                    hls.Bitrate = GetBitrate(line);
                    (hls.Width, hls.Height) = GetResolution(line);

                    line = reader.ReadLine();

                    if (!line.StartsWith("https"))
                        continue;

                    hls.Url = line;

                    playlist.Add(hls);
                }
            }

            return new HLSPlaylist { Playlist = playlist.ToArray() };
        }

        static string GetName(string line)
        {
            string pattern = @"NAME=""(.*)""";

            var match = Regex.Match(line, pattern);

            if (match is null)
                return "";

            if (match.Groups.Count == 2)
            {
                return match.Groups[1].Value;
            }

            return "";
        }

        static long GetBitrate(string line)
        {
            string pattern = @"BANDWIDTH=(\d*),";

            var match = Regex.Match(line, pattern);

            if (match is null)
                return 0;

            if (match.Groups.Count == 2)
            {
                return Convert.ToInt64(match.Groups[1].Value);
            }

            return 0;
        }

        static (int, int) GetResolution(string line)
        {
            string pattern = @"RESOLUTION=(\d*)x(\d*)";

            var match = Regex.Match(line, pattern);

            if (match is null)
                return (0,0);

            if (match.Groups.Count == 3)
            {
                int width = Convert.ToInt32(match.Groups[1].Value);
                int height = Convert.ToInt32(match.Groups[2].Value);

                return (width, height);
            }

            return (0,0);
        }
    }
}
