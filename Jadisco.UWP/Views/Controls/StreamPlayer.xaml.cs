using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Twitch.Api.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Jadisco.UWP.Views.Controls
{
    public sealed partial class StreamPlayer : UserControl
    {
        #region Private fields
        private MseStreamSource mseStreamSource;
        private MseSourceBuffer mseSourceBuffer;
        MediaSource mseMediaSource;

        HLSStream currentHLSStream;
        #endregion

        public StreamPlayer()
        {
            this.InitializeComponent();

            StreamMediaPlayer.MediaPlayer.AutoPlay = true;
        }

        #region Public methods
        /// <summary>
        /// Play specified stream
        /// </summary>
        /// <param name="hlsStream">HLSStream to play</param>
        /// <returns>Whether stream will play</returns>
        public bool PlayStream(HLSStream hlsStream)
        {
            if (hlsStream is null)
                return false;

            StreamMediaPlayer.Source = MediaSource.CreateFromUri(new Uri(hlsStream.Url));

            StreamMediaPlayer.AreTransportControlsEnabled = true;

            return true;
        }

        /// <summary>
        /// Play specified stream in low latency mode
        /// </summary>
        /// <param name="hlsStream">HLSStream to play</param>
        /// <returns>Whether stream will play</returns>
        public bool PlayLowLatencyStream(HLSStream hlsStream)
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

            StreamMediaPlayer.Source = mseMediaSource;
            StreamMediaPlayer.AreTransportControlsEnabled = true;

            return true;
        }

        public void StopStream(bool closeHLSStream = true)
        {
            if (mseMediaSource != null)
            {
                mseMediaSource?.Reset();
                mseMediaSource?.Dispose();
                mseMediaSource = null;
            }

            if (mseSourceBuffer != null)
            {
                mseSourceBuffer.Updated -= MseSourceBuffer_Updated;
                mseSourceBuffer = null;
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

            StreamMediaPlayer.Source = null;
        }

        public void PlaySplashScreen()
        {
            StreamMediaPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/SplashAssets/SplashVideo.mp4"));
            StreamMediaPlayer.AreTransportControlsEnabled = false;
        }

        public void RefreshStream()
        {
            if (currentHLSStream != null)
            {
                currentHLSStream.ClearQueue();

                StopStream(false);
                PlayLowLatencyStream(currentHLSStream);
            }
        }
        #endregion

        #region Private methods
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
                mseSourceBuffer.Updated += MseSourceBuffer_Updated;
            }
        }

        private void MseSourceBuffer_Updated(MseSourceBuffer sender, object args)
        {

        }
    }
}
