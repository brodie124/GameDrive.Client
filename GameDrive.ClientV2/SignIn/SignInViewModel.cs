using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        set {  
            SetField(ref _isLoading, value);
            OnPropertyChanged(nameof(ShowForm));
            OnPropertyChanged(nameof(ShowLoadingSpinner));
        }
    }

    public bool ShowForm => !IsLoading;
    public bool ShowLoadingSpinner => IsLoading;


    public async Task DoSignIn()
    {
        IsLoading = !IsLoading;
        await _model.SignInAsync();
    }
    

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}