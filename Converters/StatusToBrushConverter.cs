using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Avalonia.Media;

namespace curs.Converters
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var status = value as string;
            if (string.Equals(status, "Правильно", StringComparison.OrdinalIgnoreCase))
                
                return Brushes.LightGreen;
            if (string.Equals(status, "Неправильно", StringComparison.OrdinalIgnoreCase))
                return Brushes.LightCoral;
            return Brushes.Transparent;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
