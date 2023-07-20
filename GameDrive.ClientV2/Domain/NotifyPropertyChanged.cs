using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using GameDrive.ClientV2.SignIn;

namespace GameDrive.ClientV2.Domain;

public class NotifyPropertyChanged : INotifyPropertyChanged
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
}