using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ICGEMDownloader
{
    class BooleanToColorConverter : IValueConverter    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool) value ? new SolidColorBrush(Colors.LimeGreen) : parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
