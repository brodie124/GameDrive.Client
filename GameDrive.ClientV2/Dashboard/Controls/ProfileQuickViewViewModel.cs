using GameDrive.ClientV2.Domain.Models;
using GameDrive.ClientV2.SignIn;

namespace GameDrive.ClientV2.Dashboard.Controls;

public class ProfileQuickViewViewModel : ViewModelBase
{
    private GameObject? _gameObject;
    public string? ResolvedBaseDirectory => _gameObject?.Profile.BaseDirectory.ResolvedPath;
    public string? ProfileName => _gameObject?.Profile.Name;
    public GameObject? GameObject
    {
        get => _gameObject;
        set => SetField(ref _gameObject, value, PropertyChangedUpdateTrigger.All);
    }
    
    public bool IsGameObjectSelected => GameObject is not null;
}