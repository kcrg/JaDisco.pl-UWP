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
        static readonly string urlRegex = @"(https?|ftp)://[^\s/$.?#].[^\s]*";

        private Paragraph _dataSource;

        public Paragraph DataSource
        {
            get => _dataSource;
            set { _dataSource = value; NotifyPropertyChanged(); }
        }

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

        Image CreateImage(ImageSource imageSource, int offset, int padding, float scale)
        {
            var transform = new CompositeTransform
            {
                TranslateY = offset,
                ScaleX = scale / 14,
                ScaleY = scale / 14,
                CenterX = 14,
                CenterY = 14
            };

            var image = new Image
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

        Image CreateImage(string url, int offset, int padding, float scale)
        {
            return CreateImage(new BitmapImage(new Uri(url)), padding, offset, scale);
        }

        Paragraph GenerateMessage(IrcUser author, List<PoorchatUserMode> userModes, string message)
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

                        container.Child = CreateImage(subBadge.Url, 4, 4, 16f);
                        data.Inlines.Add(container);
                    }
                    else
                    {
                        var task = PoorchatApi.GetBadgeAsync(userMode);
                        task.Wait();

                        var badge = task.Result;

                        if (badge is null)
                            continue;

                        container.Child = CreateImage(badge.Url, 4, 4, 16f);
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
                if (GetUrl(word, regex, out string url))
                {
                    var hyperlink = new Hyperlink();
                    hyperlink.NavigateUri = new Uri(url);

                    var text = new Run();
                    text.Text = url;
                    hyperlink.Inlines.Add(text);

                    data.Inlines.Add(hyperlink);
                }
                else if (GetEmoticon(word, out var imageSource))
                {
                    var container = new InlineUIContainer();
                    container.FontSize = 1;
                    container.Child = CreateImage(imageSource, 6, 8, 20f);

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

        private bool GetEmoticon(string text, out ImageSource img)
        {
            img = null;

            var task = PoorchatApi.GetEmoticonAsync(text);
            task.Wait();

            var emoticon = task.Result;

            if (emoticon is null)
                return false;

            img = new BitmapImage(new Uri(emoticon.Url));

            return true;
        }

        private bool GetUrl(string text, Regex regex, out string url)
        {
            url = null;

            var match = regex.Match(text);

            if (!match.Success)
                return false;

            url = match.Groups.First().Value;

            return true;
        }
    }
}
