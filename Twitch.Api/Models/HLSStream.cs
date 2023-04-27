using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
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
        private static readonly string extinf_re = @"(\d+(\.\d+)?)";
        private static readonly string ext_twitch_prefetch_re = @":(.*)";
        private HttpClient client;
        private HLSSegment recentSegment;
        private Queue<HLSSegment> hlsQueue;
        private Task workerTask;
        private Task writerTask;
        private MemoryStream buffer;
        #endregion

        public HLSStream()
        {
        }

        #region Public methods
        public void Open()
        {
            client = new HttpClient();
            hlsQueue = new Queue<HLSSegment>();

            Status = StreamStatus.Opened;

            // 32 MB buffer
            buffer = new MemoryStream(32 * 1024 * 1024);

            workerTask = new Task(Worker);
            workerTask.Start();

            writerTask = new Task(Writer);
            writerTask.Start();
        }

        public void Close()
        {
            client.Dispose();
            client = null;
            hlsQueue = null;

            Status = StreamStatus.Closed;

            buffer.Close();
            buffer = null;
        }

        public void ClearQueue()
        {
            hlsQueue.Clear();
        }
        #endregion

        #region Private methods
        private async void Worker()
        {
            Debug.WriteLine("Worker start");

            try
            {
                while (Status == StreamStatus.Opened)
                {
                    await UpdateQueue();

                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Worker error: {ex.Message}");
            }

            Debug.WriteLine("Worker end");
        }

        private async void Writer()
        {
            Debug.WriteLine("Writer start");

            try
            {
                while (Status == StreamStatus.Opened)
                {
                    if (hlsQueue is null)
                    {
                        break;
                    }

                    if (client is null)
                    {
                        break;
                    }

                    if (hlsQueue.Count <= 0)
                    {
                        continue;
                    }

                    HLSSegment hlsStream = hlsQueue.Dequeue();

                    if (hlsStream is null)
                    {
                        continue;
                    }

                    Stream stream = await client?.GetStreamAsync(hlsStream.Url);

                    if (stream is null)
                    {
                        continue;
                    }

                    if (buffer is null)
                    {
                        break;
                    }

                    buffer.Position = 0;
                    buffer.SetLength(0);

                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        while (true)
                        {
                            byte[] bytes = reader.ReadBytes(8192);

                            int len = bytes.Length;

                            if (len == 0)
                            {
                                break;
                            }

                            if (buffer != null && buffer.CanWrite)
                            {
                                buffer.Write(bytes, 0, len);
                            }
                        }
                    }

                    OnVideoDownload?.Invoke(buffer.ToArray());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Writer error: {ex.Message}");
            }

            Debug.WriteLine("Writer end");
        }

        private async Task UpdateQueue()
        {
            if (client is null)
            {
                return;
            }

            Stream stream = await client.GetStreamAsync(Url);

            if (stream is null)
            {
                return;
            }

            HLSSegment lastSegment = ParseSegmentFile(stream)?.Last();

            if (!lastSegment.Equals(recentSegment))
            {
                //Debug.WriteLine($"New segment!");

                recentSegment = lastSegment;

                hlsQueue.Enqueue(lastSegment);
            }
        }

        private static IEnumerable<HLSSegment> ParseSegmentFile(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (line is null)
                    {
                        continue;
                    }

                    // read normal segment data
                    if (line.StartsWith("#EXTINF"))
                    {
                        HLSSegment segment = new HLSSegment
                        {
                            Duration = GetDuration(line)
                        };

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
                        HLSSegment segment = new HLSSegment
                        {
                            Prefetch = true
                        };

                        Match match = Regex.Match(line, ext_twitch_prefetch_re);

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
            Match match = Regex.Match(line, extinf_re);

            return match.Success ? float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture.NumberFormat) : 0f;
        }
        #endregion

    }
}