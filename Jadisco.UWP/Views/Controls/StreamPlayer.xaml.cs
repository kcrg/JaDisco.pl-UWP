using Jadisco.Api.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Twitch.Api;
using Twitch.Api.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Jadisco.UWP.Views.Controls
{
    public enum StreamPlayerType
    {
        NativeLowLatency,
        NativeOld,
        Web
    }

    enum StreamPlayerTypeLocal
    {
        Native,
        Web
    }

    public sealed partial class StreamPlayer : UserControl
    {
        #region Public properties
        public StreamPlayerType PlayerType { get; set; } = StreamPlayerType.NativeLowLatency;

        public HLSPlaylist HLSPlaylist { get; private set; }

        public string[] SupportedServices
        {
            get => new string[] { "twitch" };
        }
        #endregion

        #region Private fields
        private StreamPlayerTypeLocal PlayerTypeLocal
        {
            set
            {
                switch (value)
                {
                    case StreamPlayerTypeLocal.Native:
                        StreamMediaPlayerNative.Visibility = Visibility.Visible;
                        StreamMediaPlayerWeb.Visibility = Visibility.Collapsed;
                        break;
                    case StreamPlayerTypeLocal.Web:
                        StreamMediaPlayerWeb.Visibility = Visibility.Visible;
                        StreamMediaPlayerNative.Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }

        private MseStreamSource mseStreamSource;
        private MseSourceBuffer mseSourceBuffer;
        private MediaSource mseMediaSource;

        private HLSStream currentHLSStream;

        private Service currentService;

        private Uri uriToPlay;
        #endregion

        public StreamPlayer()
        {
            this.InitializeComponent();

            StreamMediaPlayerNative.MediaPlayer.AutoPlay = true;
        }

        #region Public methods
        /// <summary>
        /// Setups for playing stream
        /// </summary>
        /// <param name="service">Stream to setup</param>
        /// <returns>Whether stream will be able to play</returns>
        public bool Setup(Service service)
        {
            currentService = service;

            switch (service.ServiceName)
            {
                case "twitch":
                {
                    AccessToken token = TwitchApi.GetAccessToken(service.ChannelId);

                    if (token is null)
                    {
                        return false;
                    }

                    string url = UsherService.GetStreamLink(service.ChannelId, token.Sig, token.Token);

                    HLSPlaylist = UsherService.ParsePlaylists(url);

                    if (HLSPlaylist is null)
                    {
                        return false;
                    }

                    uriToPlay = new Uri("https://player.twitch.tv/?!muted&channel=" + currentService.ChannelId);

                    return true;
                }
                case "youtube":
                {
                    // todo
                } break;
            }

            return false;
        }

        /// <summary>
        /// Play current stream (You have to setup before)
        /// </summary>
        public bool PlayStream()
        {
            switch (PlayerType)
            {
                case StreamPlayerType.NativeLowLatency:
                    if (HLSPlaylist is null || HLSPlaylist.Playlist.Count() <= 0)
                        return false;

                    return PlayNativeLowLatency(HLSPlaylist.Playlist[0]);
                case StreamPlayerType.NativeOld:
                    if (HLSPlaylist is null || HLSPlaylist.Playlist.Count() <= 0)
                        return false;

                    return PlayNativeOld(HLSPlaylist.Playlist[0]);
                case StreamPlayerType.Web:
                    if (uriToPlay is null)
                        return false;

                    return PlayWeb(uriToPlay);
            }

            return false;
        }

        /// <summary>
        /// Play specified stream
        /// </summary>
        /// <param name="hlsStream">HLSStream to play</param>
        /// <returns>Whether stream will play</returns>
        public bool PlayHLSStream(HLSStream hlsStream)
        {
            switch (PlayerType)
            {
                case StreamPlayerType.NativeLowLatency:
                    return PlayNativeLowLatency(hlsStream);
                case StreamPlayerType.NativeOld:
                    return PlayNativeOld(hlsStream);
            }

            return false;
        }

        /// <summary>
        /// Stops stream
        /// </summary>
        /// <param name="closeHLSStream">Whether HLSStream will be closed</param>
        public void StopStream(bool closeHLSStream = true)
        {
            if (mseMediaSource != null)
            {
                mseMediaSource?.Reset();
                mseMediaSource?.Dispose();
                mseMediaSource = null;
            }

            if (mseStreamSource != null)
            {
                mseStreamSource.Opened -= MseStreamSource_Opened;
                mseStreamSource = null;
            }

            if (closeHLSStream == true && currentHLSStream != null)
            {
                currentHLSStream.Close();
                currentHLSStream.OnVideoDownload -= HlsStream_OnVideoDownload;
                currentHLSStream = null;
            }

            mseSourceBuffer = null;
            StreamMediaPlayerNative.Source = null;
            StreamMediaPlayerWeb.NavigateToString("");
        }

        /// <summary>
        /// Plays splash screen
        /// </summary>
        public void PlaySplashScreen()
        {
            return;

            PlayerTypeLocal = StreamPlayerTypeLocal.Native;

            StreamMediaPlayerNative.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/SplashAssets/SplashVideo.mp4"));
            StreamMediaPlayerNative.AreTransportControlsEnabled = false;
        }

        /// <summary>
        /// Restart stream
        /// </summary>
        public void RestartStream()
        {
            if (currentHLSStream != null)
            {
                currentHLSStream.ClearQueue();

                var closeHLS = true;

                if (PlayerType == StreamPlayerType.NativeLowLatency)
                    closeHLS = false;

                StopStream(closeHLS);

                switch (PlayerType)
                {
                    case StreamPlayerType.NativeLowLatency:
                    case StreamPlayerType.NativeOld:
                        PlayHLSStream(currentHLSStream);
                        break;
                    case StreamPlayerType.Web:
                        StreamMediaPlayerWeb?.Refresh();
                        break;
                }
            }
        }
        #endregion

        #region Private methods

        public bool PlayNativeOld(HLSStream hlsStream)
        {
            if (hlsStream is null)
                return false;

            StreamMediaPlayerNative.Source = MediaSource.CreateFromUri(new Uri(hlsStream.Url));
            StreamMediaPlayerNative.AreTransportControlsEnabled = true;

            PlayerTypeLocal = StreamPlayerTypeLocal.Native;

            return true;
        }

        public bool PlayNativeLowLatency(HLSStream hlsStream)
        {
            if (hlsStream is null)
                return false;

            if (mseMediaSource is null)
            {
                mseStreamSource = new MseStreamSource();
                mseStreamSource.Opened += MseStreamSource_Opened;

                mseMediaSource = MediaSource.CreateFromMseStreamSource(mseStreamSource);
            }

            if (hlsStream.Status == HLSStream.StreamStatus.Closed)
            {
                hlsStream.Open();
                hlsStream.OnVideoDownload += HlsStream_OnVideoDownload;

                currentHLSStream = hlsStream;
            }

            StreamMediaPlayerNative.Source = mseMediaSource;
            StreamMediaPlayerNative.AreTransportControlsEnabled = true;

            PlayerTypeLocal = StreamPlayerTypeLocal.Native;

            return true;
        }

        public bool PlayWeb(Uri url)
        {
            PlayerTypeLocal = StreamPlayerTypeLocal.Web;

            StreamMediaPlayerWeb.Navigate(url);

            return true;
        }

        private void HlsStream_OnVideoDownload(byte[] buffer)
        {
            if (mseSourceBuffer != null && !mseSourceBuffer.IsUpdating)
            {
                //Debug.WriteLine($"Data write {mseSourceBuffer.Buffered.Count}");

                mseSourceBuffer.AppendBuffer(buffer.AsBuffer());
            }
        }

        private void MseStreamSource_Opened(MseStreamSource sender, object args)
        {
            if (mseSourceBuffer is null)
            {
                mseSourceBuffer = mseStreamSource.AddSourceBuffer("video/MP2T");
                mseSourceBuffer.Mode = MseAppendMode.Sequence;
            }
        }
        #endregion
    }
}
