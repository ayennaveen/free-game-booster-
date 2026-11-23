using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ConsoleBoost.Helpers;

public class PercentageToWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is double percent && values[1] is double totalWidth)
        {
            double width = (Math.Clamp(percent, 0, 100) / 100.0) * totalWidth;
            return Math.Max(0, width);
        }
        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class UsageToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double val)
        {
            if (val > 85.0) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5252")); // Red
            if (val > 65.0) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB74D")); // Orange
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00F2FE")); // Cyan / Blue
        }
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00F2FE"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return b ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility v)
        {
            return v == Visibility.Visible;
        }
        return false;
    }
}
