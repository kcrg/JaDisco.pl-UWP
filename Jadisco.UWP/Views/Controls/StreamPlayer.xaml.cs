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

        /// <summary>
        /// Play specified stream
        /// </summary>
        /// <param name="url">Link to stream</param>
        /// <returns>Whether stream will play</returns>
        public bool PlayStream(HLSStream hlsStream)
        {
            if (hlsStream is null)
                return false;

            StreamMediaPlayer.Source = MediaSource.CreateFromUri(new Uri(hlsStream.Url));

            StreamMediaPlayer.AreTransportControlsEnabled = true;

            return true;
        }

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

            hlsStream.Open();
            hlsStream.OnVideoDownload += HlsStream_OnVideoDownload;

            currentHLSStream = hlsStream;

            StreamMediaPlayer.Source = mseMediaSource;
            StreamMediaPlayer.AreTransportControlsEnabled = true;

            return true;
        }

        private void HlsStream_OnVideoDownload(Stream stream)
        {
            if (mseSourceBuffer != null && !mseSourceBuffer.IsUpdating)
            {
                //Debug.WriteLine("Data write");

                //mseSourceBuffer.AppendBuffer(data.AsBuffer());

                stream.Position = 0;
                mseSourceBuffer.AppendStream(stream.AsInputStream());
            }
        }

        public void StopStream()
        {
            //if (mseSourceBuffer != null)
            //{
            //    mseSourceBuffer.Updated -= MseSourceBuffer_Updated;
            //    mseSourceBuffer.Abort();
            //}

            //if (mseStreamSource != null)
            //{
            //    mseStreamSource.Opened -= MseStreamSource_Opened;
            //}

            if (currentHLSStream != null)
            {
                currentHLSStream.Close();
                currentHLSStream.OnVideoDownload -= HlsStream_OnVideoDownload;
                currentHLSStream = null;
            }

            mseMediaSource?.Reset();
            mseSourceBuffer = null;
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
