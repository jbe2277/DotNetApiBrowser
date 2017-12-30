using System;
using System.Globalization;
using System.Windows.Data;

namespace Waf.DotNetApiBrowser.Presentation.Converters
{
    public class DownloadCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long count = System.Convert.ToInt64(value, CultureInfo.CurrentCulture);
            if (count < 1_000) return count;
            if (count < 1_000_000) return (count / 1_000).ToString(CultureInfo.CurrentCulture) + "k";
            if (count < 1_000_000_000) return (count / 1_000_000).ToString(CultureInfo.CurrentCulture) + "M";
            return (count / 1_000_000_000).ToString(CultureInfo.CurrentCulture) + "G";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
