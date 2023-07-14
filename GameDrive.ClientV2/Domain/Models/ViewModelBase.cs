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
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;

        switch (updateChangedTrigger)
        {
            case PropertyChangedUpdateTrigger.All:
                var siblingPropertyNames = this.GetType()
                    .GetProperties()
                    .Select(x => x.Name)
                    .ToList();

                foreach (var siblingPropertyName in siblingPropertyNames)
                    OnPropertyChanged(siblingPropertyName);

                OnPropertyChanged();
                return true;
            case PropertyChangedUpdateTrigger.NamedProperty:
                OnPropertyChanged(propertyName);
                return true;
            default:
                throw new NotImplementedException();
        }
    }

    protected void ShowMessageBox(
        ShowMessageBoxRequest showMessageBoxRequest
    )
    {
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
                showMessageBoxRequest.SecondaryButton.ClickHandler((MessageBox)sender, eventArgs);
                messageBox.Close();
            };
        }
        else
        {
            messageBox.ButtonLeftName = "Close";
            messageBox.ButtonLeftClick += (_, _) => messageBox.Close();
        }

        if (!showMessageBoxRequest.PrimaryButton.IsVisible)
            messageBox.ButtonRightAppearance = ControlAppearance.Transparent;

        if (!showMessageBoxRequest.SecondaryButton?.IsVisible ?? true)
            messageBox.ButtonLeftAppearance = ControlAppearance.Transparent;

        messageBox.Show();
    }
}

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
    public static MessageBoxButtonState CloseButton()
    {
        return new MessageBoxButtonState(
            Text: "Close",
            ClickHandler: (messageBox, _) => messageBox.Close(),
            Appearance: ControlAppearance.Secondary
        );
    }
}