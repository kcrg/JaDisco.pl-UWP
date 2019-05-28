using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Twitch.Api.Models
{
    public class HLSPlaylist
    {
        #region Public properties
        public HLSStream[] Playlist { get; set; }
        #endregion

        #region Private members
        static readonly string name_re = @"NAME=""(.*)""";
        static readonly string bitrate_re = @"BANDWIDTH=(\d*),";
        static readonly string resolution_re = @"RESOLUTION=(\d*)x(\d*)";
        #endregion

        #region Public methods
        public static HLSPlaylist GetFromStream(Stream stream)
        {
            List<HLSStream> playlist = new List<HLSStream>();

            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (!line.StartsWith("#EXT-X-MEDIA:TYPE=VIDEO"))
                    {
                        continue;
                    }

                    HLSStream hls = new HLSStream
                    {
                        Name = GetName(line)
                    };

                    line = reader.ReadLine();

                    if (!line.StartsWith("#EXT-X-STREAM-INF"))
                    {
                        continue;
                    }

                    hls.Bitrate = GetBitrate(line);
                    (hls.Width, hls.Height) = GetResolution(line);

                    line = reader.ReadLine();

                    if (!line.StartsWith("https"))
                    {
                        continue;
                    }

                    hls.Url = line;

                    playlist.Add(hls);
                }
            }

            return new HLSPlaylist { Playlist = playlist.ToArray() };
        }
        #endregion

        #region Private methods
        private static string GetName(string line)
        {
            Match match = Regex.Match(line, name_re);

            if (match is null)
            {
                return "";
            }

            if (match.Groups.Count == 2)
            {
                return match.Groups[1].Value;
            }

            return "";
        }

        private static long GetBitrate(string line)
        {
            Match match = Regex.Match(line, bitrate_re);

            if (match is null)
            {
                return 0;
            }

            if (match.Groups.Count == 2)
            {
                return Convert.ToInt64(match.Groups[1].Value);
            }

            return 0;
        }

        private static (int, int) GetResolution(string line)
        {
            Match match = Regex.Match(line, resolution_re);

            if (match is null)
            {
                return (0, 0);
            }

            if (match.Groups.Count == 3)
            {
                int width = Convert.ToInt32(match.Groups[1].Value);
                int height = Convert.ToInt32(match.Groups[2].Value);

                return (width, height);
            }

            return (0, 0);
        }
        #endregion
    }
}