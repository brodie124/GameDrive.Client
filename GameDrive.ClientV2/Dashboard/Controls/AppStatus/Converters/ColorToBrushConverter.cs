using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GameDrive.ClientV2.Dashboard.Controls.AppStatus.Converters;

public class ColorToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Color color)
            throw new InvalidOperationException($"Non-Color supplied to {nameof(ColorToBrushConverter)} (forward)");

        return new SolidColorBrush(color);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not SolidColorBrush solidColorBrush)
            throw new InvalidOperationException($"Non-SolidColorBrush supplied to {nameof(ColorToBrushConverter)} (backward)");

        return solidColorBrush.Color;
    }
}