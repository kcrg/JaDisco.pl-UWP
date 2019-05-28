using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Twitch.Api.Models
{
    public class HLSStream
    {
        public enum StreamStatus
        {
            Opened,
            Closed
        }

        #region Public fields
        public string Name { get; set; }

        public string Url { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public long Bitrate { get; set; }

        public StreamStatus Status { get; set; } = StreamStatus.Closed;

        public event Action<byte[]> OnVideoDownload;
        #endregion

        #region Private variables
        static readonly string extinf_re = @"(\d+(\.\d+)?)";
        static readonly string ext_twitch_prefetch_re = @":(.*)";

        readonly HttpClient client = new HttpClient();

        HLSSegment recentSegment;

        Queue<HLSSegment> hlsQueue = new Queue<HLSSegment>();

        Task workerTask;
        Task writerTask;
        #endregion

        public HLSStream()
        {
        }

        #region Public methods
        public void Open()
        {
            Status = StreamStatus.Opened;

            workerTask = new Task(Worker);
            workerTask.Start();

            writerTask = new Task(Writer);
            writerTask.Start();
        }

        public void Close()
        {
            Status = StreamStatus.Closed;
        }
        #endregion

        #region Private methods
        private async void Worker()
        {
            while (Status == StreamStatus.Opened)
            {
                await UpdateQueue();

                await Task.Delay(1000);
            }
        }

        private async void Writer()
        {
            while (Status == StreamStatus.Opened)
            {
                if (hlsQueue.Count == 0)
                    continue;

                var stream = await client.GetStreamAsync(hlsQueue.Dequeue().Url);

                using (MemoryStream buffer = new MemoryStream())
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        while (true)
                        {
                            var bytes = reader.ReadBytes(8192);

                            var len = bytes.Length;

                            if (len == 0)
                                break;

                            buffer.Write(bytes, 0, len);
                        }
                    }

                    OnVideoDownload?.Invoke(buffer.ToArray());
                }
            }
        }

        private async Task UpdateQueue()
        {
            var stream = await client.GetStreamAsync(Url);

            var lastSegment = ParseSegmentFile(stream)?.Last();

            if (!lastSegment.Equals(recentSegment))
            {
                //Debug.WriteLine($"New segment!");

                recentSegment = lastSegment;

                hlsQueue.Enqueue(lastSegment);
            }
        }

        private static IEnumerable<HLSSegment> ParseSegmentFile(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    if (line is null)
                    {
                        continue;
                    }

                    // read normal segment data
                    if (line.StartsWith("#EXTINF"))
                    {
                        var segment = new HLSSegment();

                        segment.Duration = GetDuration(line);

                        // read next line with url
                        line = reader.ReadLine();

                        // skip if this line isn't link
                        if (line == null || !line.StartsWith("https"))
                        {
                            continue;
                        }

                        segment.Url = line;

                        yield return segment;
                    }

                    // read prefetch
                    if (line.StartsWith("#EXT-X-TWITCH-PREFETCH"))
                    {
                        var segment = new HLSSegment();

                        segment.Prefetch = true;

                        var match = Regex.Match(line, ext_twitch_prefetch_re);

                        if (match.Success)
                        {
                            segment.Url = match.Groups[1].Value;
                        }

                        yield return segment;
                    }
                }
            }

        }

        private static float GetDuration(string line)
        {
            var match = Regex.Match(line, extinf_re);

            if (match.Success)
            {
                return float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture.NumberFormat);
            }

            return 0f;
        }
        #endregion

    }
}