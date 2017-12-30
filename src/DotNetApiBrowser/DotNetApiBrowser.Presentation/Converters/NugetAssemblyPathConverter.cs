using System;
using System.Globalization;
using System.Windows.Data;

namespace Waf.DotNetApiBrowser.Presentation.Converters
{
    public class NugetAssemblyPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Uri.UnescapeDataString((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
