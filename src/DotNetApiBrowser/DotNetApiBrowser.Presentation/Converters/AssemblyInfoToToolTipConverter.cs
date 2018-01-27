using System;
using System.Globalization;
using System.Windows.Data;
using Waf.DotNetApiBrowser.Applications.DataModels;

namespace Waf.DotNetApiBrowser.Presentation.Converters
{
    public class AssemblyInfoToToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is AssemblyInfo info ? info.FileName + Environment.NewLine + "Version: " + info.Version : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
