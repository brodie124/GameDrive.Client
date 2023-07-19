using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using GameDrive.ClientV2.SignIn;
using Wpf.Ui.Common;
using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace GameDrive.ClientV2.Domain.Models;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected virtual bool SetField<T>(
        ref T field,
        T value,
        PropertyChangedUpdateTrigger updateChangedTrigger = PropertyChangedUpdateTrigger.NamedProperty,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) 
            return false;
        field = value;

        TriggerPropertyUpdate(updateChangedTrigger, propertyName);
        return true;
    }

    protected virtual void TriggerPropertyUpdate(
        PropertyChangedUpdateTrigger updateChangedTrigger,
        string? propertyName = null
    )
    {
        switch (updateChangedTrigger)
        {
            case PropertyChangedUpdateTrigger.All:
                var siblingPropertyNames = this.GetType()
                    .GetProperties()
                    .Select(x => x.Name)
                    .ToList();

                foreach (var siblingPropertyName in siblingPropertyNames)
                    OnPropertyChanged(siblingPropertyName);
                break;
            case PropertyChangedUpdateTrigger.NamedProperty:
                ArgumentNullException.ThrowIfNull(propertyName);
                OnPropertyChanged(propertyName);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    protected ShowMessageBoxResult ShowMessageBox(
        ShowMessageBoxRequest showMessageBoxRequest
    )
    {
        var isPrimaryClicked = false;
        var isSecondaryClicked = false;
        
        var messageBox = new MessageBox
        {
            Content = showMessageBoxRequest.Content,
            Title = showMessageBoxRequest.Title,
            ShowTitle = showMessageBoxRequest.ShowTitle,
            ShowFooter = showMessageBoxRequest.ShowFooter,
            ShowInTaskbar = showMessageBoxRequest.ShowInTaskbar,
            ButtonRightName = showMessageBoxRequest.PrimaryButton.Text,
            ButtonRightAppearance = showMessageBoxRequest.PrimaryButton.Appearance ?? ControlAppearance.Primary
        };
        
        messageBox.ButtonRightClick += (object sender, RoutedEventArgs eventArgs) =>
        {
            isPrimaryClicked = true;
            showMessageBoxRequest.PrimaryButton.ClickHandler((MessageBox)sender, eventArgs);
            messageBox.Close();
        };

        messageBox.ButtonRightAppearance = showMessageBoxRequest.PrimaryButton.Appearance ?? ControlAppearance.Primary;
        messageBox.ButtonLeftAppearance =
            showMessageBoxRequest.SecondaryButton?.Appearance ?? ControlAppearance.Secondary;
        
        if (showMessageBoxRequest.SecondaryButton is not null)
        {
            messageBox.ButtonLeftName = showMessageBoxRequest.SecondaryButton.Text;
            messageBox.ButtonLeftClick += (sender, eventArgs) =>
            {
                isSecondaryClicked = true;
                showMessageBoxRequest.SecondaryButton.ClickHandler((MessageBox)sender, eventArgs);
                messageBox.Close();
            };
        }
        else
        {
            isSecondaryClicked = true;
            messageBox.ButtonLeftName = "Close";
            messageBox.ButtonLeftClick += (_, _) => messageBox.Close();
        }

        if (!showMessageBoxRequest.PrimaryButton.IsVisible)
            messageBox.ButtonRightAppearance = ControlAppearance.Transparent;

        if (!showMessageBoxRequest.SecondaryButton?.IsVisible ?? true)
            messageBox.ButtonLeftAppearance = ControlAppearance.Transparent;

        messageBox.ShowDialog();
        return new ShowMessageBoxResult(isPrimaryClicked, isSecondaryClicked);
    }
}

public record ShowMessageBoxResult(
    bool IsPrimaryClicked,
    bool IsSecondaryClicked
);

public record ShowMessageBoxRequest(
    object Content,
    string Title,
    MessageBoxButtonState PrimaryButton,
    MessageBoxButtonState? SecondaryButton = null,
    bool ShowTitle = true,
    bool ShowFooter = true,
    bool ShowInTaskbar = true
);

public record MessageBoxButtonState(
    string Text,
    Action<MessageBox, RoutedEventArgs> ClickHandler,
    ControlAppearance? Appearance = null,
    bool IsVisible = true
)
{
    private static readonly Action<MessageBox, RoutedEventArgs> EmptyClickAction = (_, _) => { };

    public static MessageBoxButtonState CloseButton(
        Action<MessageBox, RoutedEventArgs>? clickAction = null, 
        ControlAppearance controlAppearance = ControlAppearance.Secondary
    )
    {
        return new MessageBoxButtonState(
            Text: "Close",
            ClickHandler: clickAction ?? EmptyClickAction,
            Appearance: controlAppearance
        );
    }
    
    public static MessageBoxButtonState YesButton(
        Action<MessageBox, RoutedEventArgs>? clickAction = null, 
        ControlAppearance controlAppearance = ControlAppearance.Primary
    )
    {
        return new MessageBoxButtonState(
            Text: "Yes",
            ClickHandler: clickAction ?? EmptyClickAction,
            Appearance: controlAppearance
        );
    }
    
    public static MessageBoxButtonState NoButton(
        Action<MessageBox, RoutedEventArgs>? clickAction = null, 
        ControlAppearance controlAppearance = ControlAppearance.Secondary
    )
    {
        return new MessageBoxButtonState(
            Text: "No",
            ClickHandler: clickAction ?? EmptyClickAction,
            Appearance: controlAppearance
        );
    }

    public static MessageBoxButtonState CancelButton(
        Action<MessageBox, RoutedEventArgs>? clickAction = null, 
        ControlAppearance controlAppearance = ControlAppearance.Secondary
    )
    {
        return new MessageBoxButtonState(
            Text: "Cancel",
            ClickHandler: clickAction ?? EmptyClickAction,
            Appearance: controlAppearance
        );
    }
}