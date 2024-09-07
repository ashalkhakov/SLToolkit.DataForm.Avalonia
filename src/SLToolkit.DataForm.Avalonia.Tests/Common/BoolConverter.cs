using Avalonia.Data.Converters;
using System.Globalization;

namespace System.Windows.Controls.UnitTests
{
    public class BoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return value.ToString();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                return bool.Parse((string)value);
            }
            return null;
        }
    }
}
