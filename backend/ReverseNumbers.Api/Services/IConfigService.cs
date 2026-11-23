namespace ReverseNumbers.Api.Services;

public interface IConfigService
{
    Task<string> GetValueAsync(string key);
    Task<int> GetIntValueAsync(string key, int defaultValue = 0);
    Task<Dictionary<string, string>> GetAllAsync();
    Task SetValueAsync(string key, string value);
}
