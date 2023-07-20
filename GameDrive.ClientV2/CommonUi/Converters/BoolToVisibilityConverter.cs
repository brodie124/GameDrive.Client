using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GameDrive.ClientV2.CommonUi.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool valueBool)
            return Visibility.Visible;

        return valueBool
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Visibility valueVisibility)
            return Visibility.Visible;

        return valueVisibility switch
        {
            Visibility.Visible => true,
            Visibility.Hidden
                or Visibility.Collapsed
                or _ => false
        };
    }
}