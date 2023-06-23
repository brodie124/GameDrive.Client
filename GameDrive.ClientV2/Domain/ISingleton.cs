namespace GameDrive.ClientV2.Domain;

public abstract class Singleton<T> where T : new()
{
    private static T? _instance = default(T);
    
    public static T GetInstance()
    {
        if (_instance is not null)
            return _instance;

        _instance = new T();
        return _instance;
    }
}