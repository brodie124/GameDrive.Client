using System;
using System.Windows;
using Wpf.Ui.Common;
using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace GameDrive.ClientV2.Domain.Models;

public record ActionButton(
    string Text,
    Action<MessageBox, RoutedEventArgs> ClickHandler,
    ControlAppearance? Appearance = null,
    bool IsVisible = true
)
{
    private static readonly Action<MessageBox, RoutedEventArgs> EmptyClickAction = (_, _) => { };

    public static ActionButton CloseButton(
        Action<MessageBox, RoutedEventArgs>? clickAction = null, 
        ControlAppearance controlAppearance = ControlAppearance.Secondary
    )
    {
        return new ActionButton(
            Text: "Close",
            ClickHandler: clickAction ?? EmptyClickAction,
            Appearance: controlAppearance
        );
    }
    
    public static ActionButton YesButton(
        Action<MessageBox, RoutedEventArgs>? clickAction = null, 
        ControlAppearance controlAppearance = ControlAppearance.Primary
    )
    {
        return new ActionButton(
            Text: "Yes",
            ClickHandler: clickAction ?? EmptyClickAction,
            Appearance: controlAppearance
        );
    }
    
    public static ActionButton NoButton(
        Action<MessageBox, RoutedEventArgs>? clickAction = null, 
        ControlAppearance controlAppearance = ControlAppearance.Secondary
    )
    {
        return new ActionButton(
            Text: "No",
            ClickHandler: clickAction ?? EmptyClickAction,
            Appearance: controlAppearance
        );
    }

    public static ActionButton CancelButton(
        Action<MessageBox, RoutedEventArgs>? clickAction = null, 
        ControlAppearance controlAppearance = ControlAppearance.Secondary
    )
    {
        return new ActionButton(
            Text: "Cancel",
            ClickHandler: clickAction ?? EmptyClickAction,
            Appearance: controlAppearance
        );
    }
}