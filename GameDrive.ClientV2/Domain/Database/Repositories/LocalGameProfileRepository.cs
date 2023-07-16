using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameDrive.ClientV2.Domain.Database.DataAccess;
using GameDrive.ClientV2.Domain.Models;

namespace GameDrive.ClientV2.Domain.Database.Repositories;

public interface ILocalGameProfileRepository
{
    Task<LocalGameProfile?> AddOrUpdateAsync(LocalGameProfile localGameProfile);
    Task<List<LocalGameProfile>> GetAllAsync();
}

public class LocalGameProfileRepository : ILocalGameProfileRepository
{
    private readonly ILocalGameProfileDataAccess _localGameProfileDataAccess;

    public LocalGameProfileRepository(ILocalGameProfileDataAccess localGameProfileDataAccess)
    {
        _localGameProfileDataAccess = localGameProfileDataAccess;
    }
    
    public async Task<LocalGameProfile?> AddOrUpdateAsync(LocalGameProfile localGameProfile)
    {
        var result = await _localGameProfileDataAccess.AddOrUpdateAsync(localGameProfile);
        return result.TryGetValue(out var value)
            ? value 
            : null;
    }

    public async Task<List<LocalGameProfile>> GetAllAsync()
    {
        var result = await _localGameProfileDataAccess.GetAllAsync();
        return result.TryGetValue(out var value)
            ? value 
            : Array.Empty<LocalGameProfile>().ToList();
    }
}