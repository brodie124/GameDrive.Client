using System;
using System.Windows;
using Wpf.Ui.Common;
using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace GameDrive.ClientV2.Domain.Models;

public record ActionButton(
    string Text,
    Action<object, EventArgs> ClickHandler,
    ControlAppearance? Appearance = null,
    bool IsVisible = true
)
{
    private static readonly Action<object, EventArgs> EmptyClickAction = (_, _) => { };

    public static ActionButton CloseButton(
        Action<object, EventArgs>? clickAction = null, 
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
        Action<object, EventArgs>? clickAction = null, 
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
        Action<object, EventArgs>? clickAction = null, 
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
        Action<object, EventArgs>? clickAction = null, 
        ControlAppearance controlAppearance = ControlAppearance.Secondary
    )
    {
        return new ActionButton(
            Text: "Cancel",
            ClickHandler: clickAction ?? EmptyClickAction,
            Appearance: controlAppearance
        );
    }
    
    public static ActionButton InvisibleButton()
    {
        return new ActionButton(
            Text: "Invisible",
            ClickHandler: EmptyClickAction,
            Appearance: ControlAppearance.Transparent,
            IsVisible: false
        );
    }
}