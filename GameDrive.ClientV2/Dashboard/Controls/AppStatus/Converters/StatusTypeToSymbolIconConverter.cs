using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using GameDrive.ClientV2.Domain.Status;
using Wpf.Ui.Markup;

namespace GameDrive.ClientV2.Dashboard.Controls.AppStatus.Converters;

public class StatusTypeToSymbolIconConverter  : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not StatusType statusType)
            throw new InvalidOperationException($"Invalid value passed to {nameof(StatusTypeToSymbolForegroundConverter)}");

        return statusType switch
        {
            StatusType.Informational => "Info24", 
            StatusType.Success => "CheckmarkCircle24",
            StatusType.Warning => "ErrorCircle24",
            StatusType.Error => "DismissCircle24",
            _ => throw new InvalidEnumArgumentException("Unhanded enum value supplied")
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    private object GetFromThemeResource(ThemeResource themeResource)
    {
        return Application.Current.Resources[themeResource.ToString()];
    }
}