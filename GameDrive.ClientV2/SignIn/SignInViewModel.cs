using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace GameDrive.ClientV2.SignIn;

public class SignInViewModel : INotifyPropertyChanged
{
    private readonly SignInModel _model = new SignInModel(); 
    
    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _isLoading = false;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Username
    {
        get => _username;
        set => SetField(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetField(ref _password, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetField(ref _isLoading, value, PropertyChangedUpdateTrigger.All);
    }

    public bool ShowForm => !IsLoading;
    public bool ShowLoadingSpinner => IsLoading;


    public async Task DoSignIn()
    {
        IsLoading = true;
        await _model.SignInAsync();
        IsLoading = false;
    }
    

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(
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

                foreach(var siblingPropertyName in siblingPropertyNames)
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
}

public enum PropertyChangedUpdateTrigger
{
    All,
    NamedProperty
}