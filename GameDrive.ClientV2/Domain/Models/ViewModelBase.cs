using System;
using System.Windows;
using Wpf.Ui.Common;
using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace GameDrive.ClientV2.Domain.Models;

public abstract class ViewModelBase : NotifyPropertyChanged
{
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
    
    protected Action<object, EventArgs> MapToMessageBoxAction(Action<MessageBox, RoutedEventArgs> action)
    {
        return (sender, args) =>
        {
            if (sender is not MessageBox messageBox || args is not RoutedEventArgs routedEventArgs)
                return;

            action(messageBox, routedEventArgs);
        };
    }
}

public record ShowMessageBoxResult(
    bool IsPrimaryClicked,
    bool IsSecondaryClicked
);

public record ShowMessageBoxRequest(
    object Content,
    string Title,
    ActionButton PrimaryButton,
    ActionButton? SecondaryButton = null,
    bool ShowTitle = true,
    bool ShowFooter = true,
    bool ShowInTaskbar = true
);