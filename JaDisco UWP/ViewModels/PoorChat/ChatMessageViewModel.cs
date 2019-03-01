using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

using IrcDotNet;
using Jadisco.Api;

namespace JaDisco_UWP.ViewModels.Poorchat
{
    public class ChatMessageViewModel : BaseViewModel
    {
        #region Private enums
        enum ImageType
        {
            Emoticon,
            Badge
        }
        #endregion

        #region Static members
        static readonly string urlRegex = @"(https?|ftp)://[^\s/$.?#].[^\s]*";

        static Dictionary<string, ImageSource> ImageSourceCache = new Dictionary<string, ImageSource>();
        #endregion

        #region Private members
        private Paragraph _dataSource;
        #endregion

        #region Public members
        public Paragraph DataSource
        {
            get => _dataSource;
            set { _dataSource = value; NotifyPropertyChanged(); }
        }
        #endregion

        #region Public methods
        public ChatMessageViewModel(IrcUser author, PoorchatUserMode[] userModes, string message)
        {
            DataSource = GenerateMessage(author, userModes.ToList(), message);

            /*
            foreach (var url in GetUrls(message))
            {
                var uri = new Uri(url);

                var request = WebRequest.Create(uri);

                request.Method = "HEAD";

                using (var response = request.GetResponse())
                {
                    if (response.ContentType.ToLower(CultureInfo.InvariantCulture).StartsWith("image/"))
                    {
                        _image = new BitmapImage(uri);
                    }
                }
            }*/
        }
        #endregion

        #region Private methods
        private Paragraph GenerateMessage(IrcUser author, List<PoorchatUserMode> userModes, string message)
        {
            var data = new Paragraph();

            if (userModes.Count > 0)
            {
                foreach (var userMode in userModes)
                {
                    var container = new InlineUIContainer();
                    container.FontSize = 1;

                    if (userMode == PoorchatUserMode.Subscriber)
                    {
                        var task = PoorchatApi.GetSubscriberBadgeAsync(1);
                        task.Wait();

                        var subBadge = task.Result;

                        if (subBadge is null)
                            continue;

                        container.Child = GetImage(subBadge.Url, ImageType.Badge);
                        data.Inlines.Add(container);
                    }
                    else
                    {
                        var task = PoorchatApi.GetBadgeAsync(userMode);
                        task.Wait();

                        var badge = task.Result;

                        if (badge is null)
                            continue;

                        container.Child = GetImage(badge.Url, ImageType.Badge);
                        data.Inlines.Add(container);
                    }
                }
            }

            var authorRun = new Run
            {
                FontWeight = FontWeights.Bold,
                Text = author.NickName + ":"
            };

            data.Inlines.Add(authorRun);

            var regex = new Regex(urlRegex);

            foreach (var word in message.Split(' '))
            {
                if (TryGetUrl(word, regex, out string url))
                {
                    var hyperlink = new Hyperlink();
                    hyperlink.NavigateUri = new Uri(url);

                    var text = new Run
                    {
                        Text = url
                    };
                    hyperlink.Inlines.Add(text);

                    data.Inlines.Add(hyperlink);
                }
                else if (TryGetEmoticon(word, out string imageUrl))
                {
                    var container = new InlineUIContainer
                    {
                        FontSize = 1,
                        Child = GetImage(imageUrl, ImageType.Emoticon)
                    };

                    data.Inlines.Add(container);
                }
                else
                {
                    var run = new Run();
                    run.Text = " " + word;
                    data.Inlines.Add(run);
                }
            }

            return data;
        }
        #endregion

        #region Helpers
        Image GetImage(string url, ImageType imageType)
        {
            Image image = null;
            ImageSource imageSource = null;

            if (!ImageSourceCache.TryGetValue(url, out imageSource))
            {
                imageSource = new BitmapImage(new Uri(url));
                ImageSourceCache[url] = imageSource;
            }

            int offset = 0, padding = 0;
            float scale = 0f;

            switch (imageType)
            {
                case ImageType.Emoticon:
                {
                    offset = 6;
                    padding = 8;
                    scale = 20f;
                } break;
                case ImageType.Badge:
                {
                    offset = 4;
                    padding = 4;
                    scale = 16f;
                } break;
            }

            var transform = new CompositeTransform
            {
                TranslateY = offset,
                ScaleX = scale / 14,
                ScaleY = scale / 14,
                CenterX = 14,
                CenterY = 14
            };

            image = new Image
            {
                Width = 16,
                Height = 16,
                Margin = new Thickness(padding, 0, padding, 0),
                Source = imageSource,
                RenderTransform = transform,
                VerticalAlignment = VerticalAlignment.Center
            };

            return image;
        }

        private bool TryGetEmoticon(string text, out string url)
        {
            url = null;

            var task = PoorchatApi.GetEmoticonAsync(text);
            task.Wait();

            var emoticon = task.Result;

            if (emoticon is null)
                return false;

            url = emoticon.Url;

            return true;
        }

        private bool TryGetUrl(string text, Regex regex, out string url)
        {
            url = null;

            var match = regex.Match(text);

            if (!match.Success)
                return false;

            url = match.Groups.First().Value;

            return true;
        }
        #endregion
    }
}
