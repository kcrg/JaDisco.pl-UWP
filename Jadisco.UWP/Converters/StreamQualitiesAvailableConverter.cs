using Jadisco.UWP.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace Jadisco.UWP.Converters
{
    public class StreamQualitiesAvailableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            IEnumerable<StreamQualityViewModel> obj = value as IEnumerable<StreamQualityViewModel>;

            return obj.Any() ? false : (object)true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
