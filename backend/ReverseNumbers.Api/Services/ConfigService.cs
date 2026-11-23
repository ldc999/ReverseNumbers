using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Entities;
using SqlSugar;

namespace ReverseNumbers.Api.Services;

public class ConfigService : IConfigService
{
    private readonly ISqlSugarClient _db;

    public ConfigService(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<string> GetValueAsync(string key)
    {
        var config = await _db.Queryable<SystemConfig>()
            .Where(c => c.ConfigKey == key)
            .FirstAsync();
        return config?.ConfigValue ?? string.Empty;
    }

    public async Task<int> GetIntValueAsync(string key, int defaultValue = 0)
    {
        var value = await GetValueAsync(key);
        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    public async Task<Dictionary<string, string>> GetAllAsync()
    {
        var list = await _db.Queryable<SystemConfig>().ToListAsync();
        return list.ToDictionary(c => c.ConfigKey, c => c.ConfigValue);
    }

    public async Task SetValueAsync(string key, string value)
    {
        var config = await _db.Queryable<SystemConfig>()
            .Where(c => c.ConfigKey == key)
            .FirstAsync();

        if (config != null)
        {
            config.ConfigValue = value;
            config.UpdatedAt = DateTime.Now;
            await _db.Updateable(config).ExecuteCommandAsync();
        }
        else
        {
            throw new AppException($"配置项 {key} 不存在");
        }
    }
}
