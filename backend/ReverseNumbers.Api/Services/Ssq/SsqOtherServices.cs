using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Dtos;
using ReverseNumbers.Api.Models.Entities;
using SqlSugar;

namespace ReverseNumbers.Api.Services.Ssq;

// ── History ──
public interface ISsqHistoryService
{
    Task<HistoryListDto> SaveAsync(HistorySaveDto dto);
    Task<List<HistoryListDto>> GetByIssueIdAsync(int issueId);
    Task<HistoryListDto?> GetByIdAsync(int id);
    Task DeleteAsync(int id);
    Task<HistoryStatsDto> GetStatsAsync(DateTime start, DateTime end);
}

public class SsqHistoryService : ISsqHistoryService
{
    private readonly ISqlSugarClient _db;
    public SsqHistoryService(ISqlSugarClient db) => _db = db;

    public async Task<HistoryListDto> SaveAsync(HistorySaveDto dto)
    {
        var e = new SsqSavedNumberHistory
        {
            IssueId = dto.IssueId,
            RedNumbers = string.Join(",", dto.FrontNumbers.Select(n => n.ToString("D2"))),
            BlueNumbers = string.Join(",", dto.BackNumbers.Select(n => n.ToString("D2"))),
            IsGenerated = dto.IsGenerated,
            MatchDetails = dto.MatchDetails,
            IterationCount = dto.IterationCount,
            CreatedAt = DateTime.Now
        };
        await _db.Insertable(e).ExecuteCommandAsync();
        return ToDto(e);
    }

    public async Task<List<HistoryListDto>> GetByIssueIdAsync(int issueId)
        => (await _db.Queryable<SsqSavedNumberHistory>().Where(h => h.IssueId == issueId).OrderBy(h => h.CreatedAt, OrderByType.Desc).ToListAsync()).Select(ToDto).ToList();

    public async Task<HistoryListDto?> GetByIdAsync(int id)
        => ToDto(await _db.Queryable<SsqSavedNumberHistory>().InSingleAsync(id));

    public async Task DeleteAsync(int id)
        => await _db.Deleteable<SsqSavedNumberHistory>().Where(h => h.Id == id).ExecuteCommandAsync();

    public async Task<HistoryStatsDto> GetStatsAsync(DateTime start, DateTime end)
    {
        var list = await _db.Queryable<SsqSavedNumberHistory>().Where(h => h.CreatedAt >= start && h.CreatedAt <= end).ToListAsync();
        var redMap = new Dictionary<int, int>(); var blueMap = new Dictionary<int, int>();
        for (int i = 1; i <= 33; i++) redMap[i] = 0;
        for (int i = 1; i <= 16; i++) blueMap[i] = 0;
        foreach (var h in list)
        {
            foreach (var n in h.RedNumbers.Split(',').Select(int.Parse)) if (redMap.ContainsKey(n)) redMap[n]++;
            foreach (var n in h.BlueNumbers.Split(',').Select(int.Parse)) if (blueMap.ContainsKey(n)) blueMap[n]++;
        }
        var total = list.Count == 0 ? 1 : list.Count;
        return new HistoryStatsDto
        {
            TotalSets = list.Count,
            FrontStats = redMap.Select(kv => new NumberStatItem { Num = kv.Key.ToString("D2"), Count = kv.Value, Pct = (kv.Value * 100.0 / total).ToString("F1") }).OrderByDescending(x => x.Count).ToList(),
            BackStats = blueMap.Select(kv => new NumberStatItem { Num = kv.Key.ToString("D2"), Count = kv.Value, Pct = (kv.Value * 100.0 / total).ToString("F1") }).OrderByDescending(x => x.Count).ToList()
        };
    }

    private static HistoryListDto ToDto(SsqSavedNumberHistory e) => new()
    {
        Id = e.Id, IssueId = e.IssueId,
        FrontNumbers = e.RedNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList(),
        BackNumbers = e.BlueNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList(),
        IsGenerated = e.IsGenerated, MatchDetails = e.MatchDetails, IterationCount = e.IterationCount, CreatedAt = e.CreatedAt
    };
}

// ── Config ──
public interface ISsqConfigService
{
    Task<string> GetValueAsync(string key);
    Task<int> GetIntValueAsync(string key, int defaultValue = 0);
    Task<Dictionary<string, string>> GetAllAsync();
    Task SetValueAsync(string key, string value);
}

public class SsqConfigService : ISsqConfigService
{
    private readonly ISqlSugarClient _db;
    public SsqConfigService(ISqlSugarClient db) => _db = db;

    public async Task<string> GetValueAsync(string key)
        => (await _db.Queryable<SsqSystemConfig>().Where(c => c.ConfigKey == key).FirstAsync())?.ConfigValue ?? string.Empty;

    public async Task<int> GetIntValueAsync(string key, int defaultValue = 0)
        => int.TryParse(await GetValueAsync(key), out var r) ? r : defaultValue;

    public async Task<Dictionary<string, string>> GetAllAsync()
        => (await _db.Queryable<SsqSystemConfig>().ToListAsync()).ToDictionary(c => c.ConfigKey, c => c.ConfigValue);

    public async Task SetValueAsync(string key, string value)
    {
        var c = await _db.Queryable<SsqSystemConfig>().Where(x => x.ConfigKey == key).FirstAsync();
        if (c != null) { c.ConfigValue = value; c.UpdatedAt = DateTime.Now; await _db.Updateable(c).ExecuteCommandAsync(); }
        else throw new AppException($"配置项 {key} 不存在");
    }
}

// ── RetainedNumber ──
public interface ISsqRetainedNumberService
{
    Task<RetainedNumberDto> SaveAsync(int issueId, NumberSaveDto dto);
    Task<List<RetainedNumberDto>> GetByIssueIdAsync(int issueId);
    Task DeleteAsync(int id);
}

public class SsqRetainedNumberService : ISsqRetainedNumberService
{
    private readonly ISqlSugarClient _db;
    public SsqRetainedNumberService(ISqlSugarClient db) => _db = db;

    public async Task<RetainedNumberDto> SaveAsync(int issueId, NumberSaveDto dto)
    {
        var e = new SsqRetainedNumber
        {
            IssueId = issueId,
            RedNumbers = string.Join(",", dto.FrontNumbers.Select(n => n.ToString("D2"))),
            BlueNumbers = string.Join(",", dto.BackNumbers.Select(n => n.ToString("D2"))),
            CreatedAt = DateTime.Now
        };
        await _db.Insertable(e).ExecuteCommandAsync();
        return ToDto(e);
    }

    public async Task<List<RetainedNumberDto>> GetByIssueIdAsync(int issueId)
        => (await _db.Queryable<SsqRetainedNumber>().Where(r => r.IssueId == issueId).OrderBy(r => r.CreatedAt, OrderByType.Desc).ToListAsync()).Select(ToDto).ToList();

    public async Task DeleteAsync(int id)
        => await _db.Deleteable<SsqRetainedNumber>().Where(r => r.Id == id).ExecuteCommandAsync();

    private static RetainedNumberDto ToDto(SsqRetainedNumber e) => new()
    {
        Id = e.Id, IssueId = e.IssueId,
        FrontNumbers = e.RedNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList(),
        BackNumbers = e.BlueNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList(),
        CreatedAt = e.CreatedAt
    };
}

// ── FavoriteNumber ──
public interface ISsqFavoriteNumberService
{
    Task<FavoriteNumberDto> SaveAsync(NumberSaveDto dto, string? label);
    Task<List<FavoriteNumberDto>> GetAllAsync();
    Task DeleteAsync(int id);
}

public class SsqFavoriteNumberService : ISsqFavoriteNumberService
{
    private readonly ISqlSugarClient _db;
    public SsqFavoriteNumberService(ISqlSugarClient db) => _db = db;

    public async Task<FavoriteNumberDto> SaveAsync(NumberSaveDto dto, string? label)
    {
        var e = new SsqFavoriteNumber
        {
            RedNumbers = string.Join(",", dto.FrontNumbers.Select(n => n.ToString("D2"))),
            BlueNumbers = string.Join(",", dto.BackNumbers.Select(n => n.ToString("D2"))),
            Label = label, CreatedAt = DateTime.Now
        };
        await _db.Insertable(e).ExecuteCommandAsync();
        return ToDto(e);
    }

    public async Task<List<FavoriteNumberDto>> GetAllAsync()
        => (await _db.Queryable<SsqFavoriteNumber>().OrderBy(f => f.CreatedAt, OrderByType.Desc).ToListAsync()).Select(ToDto).ToList();

    public async Task DeleteAsync(int id)
        => await _db.Deleteable<SsqFavoriteNumber>().Where(f => f.Id == id).ExecuteCommandAsync();

    private static FavoriteNumberDto ToDto(SsqFavoriteNumber e) => new()
    {
        Id = e.Id,
        FrontNumbers = e.RedNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList(),
        BackNumbers = e.BlueNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList(),
        Label = e.Label, CreatedAt = e.CreatedAt
    };
}
