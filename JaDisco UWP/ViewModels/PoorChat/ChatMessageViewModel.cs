using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace JaDisco_UWP.ViewModels.PoorChat
{
    public class ChatMessageViewModel : BaseViewModel
    {
        static readonly string urlRegex = @"(https?|ftp)://[^\s/$.?#].[^\s]*";

        private string _author;
        private string _message;
        private ImageSource _image;

        public string Author
        {
            get => _author;
            set { _author = value; NotifyPropertyChanged(); }
        }

        public string Message
        {
            get => _message;
            set { _message = value; NotifyPropertyChanged(); }
        }

        public ImageSource Image
        {
            get => _image;
            set { _image = value; NotifyPropertyChanged(); }
        }

        public ChatMessageViewModel(string author, string message)
        {
            Author = author;
            Message = message;

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
            }
        }

        private IEnumerable<string> GetUrls(string text)
        {
            var regex = new Regex(urlRegex);

            var matches = regex.Matches(text);

            foreach (Match match in matches)
            {
                yield return match.Groups[0].Value;
            }
        }
    }
}
