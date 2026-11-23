using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Dtos;
using ReverseNumbers.Api.Models.Entities;
using SqlSugar;

namespace ReverseNumbers.Api.Services;

public class HistoryService : IHistoryService
{
    private readonly ISqlSugarClient _db;

    public HistoryService(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<HistoryListDto> SaveAsync(HistorySaveDto dto)
    {
        var entity = new SavedNumberHistory
        {
            IssueId = dto.IssueId,
            FrontNumbers = string.Join(",", dto.FrontNumbers.Select(n => n.ToString("D2"))),
            BackNumbers = string.Join(",", dto.BackNumbers.Select(n => n.ToString("D2"))),
            IsGenerated = dto.IsGenerated,
            MatchDetails = dto.MatchDetails,
            IterationCount = dto.IterationCount,
            CreatedAt = DateTime.Now
        };
        await _db.Insertable(entity).ExecuteCommandAsync();
        return ToDto(entity);
    }

    public async Task<List<HistoryListDto>> GetByIssueIdAsync(int issueId)
    {
        var list = await _db.Queryable<SavedNumberHistory>()
            .Where(h => h.IssueId == issueId)
            .OrderBy(h => h.CreatedAt, OrderByType.Desc)
            .ToListAsync();
        return list.Select(ToDto).ToList();
    }

    public async Task<HistoryListDto?> GetByIdAsync(int id)
    {
        var entity = await _db.Queryable<SavedNumberHistory>().InSingleAsync(id);
        return entity == null ? null : ToDto(entity);
    }

    public async Task DeleteAsync(int id)
    {
        await _db.Deleteable<SavedNumberHistory>().Where(h => h.Id == id).ExecuteCommandAsync();
    }

    public async Task<HistoryStatsDto> GetStatsAsync(DateTime start, DateTime end)
    {
        var list = await _db.Queryable<SavedNumberHistory>()
            .Where(h => h.CreatedAt >= start && h.CreatedAt <= end)
            .ToListAsync();

        var frontMap = new Dictionary<int, int>();
        var backMap = new Dictionary<int, int>();
        for (int i = 1; i <= 35; i++) frontMap[i] = 0;
        for (int i = 1; i <= 12; i++) backMap[i] = 0;

        foreach (var h in list)
        {
            foreach (var n in h.FrontNumbers.Split(',').Select(int.Parse))
                if (frontMap.ContainsKey(n)) frontMap[n]++;
            foreach (var n in h.BackNumbers.Split(',').Select(int.Parse))
                if (backMap.ContainsKey(n)) backMap[n]++;
        }

        var total = list.Count == 0 ? 1 : list.Count;
        return new HistoryStatsDto
        {
            TotalSets = list.Count,
            FrontStats = frontMap.Select(kv => new NumberStatItem { Num = kv.Key.ToString("D2"), Count = kv.Value, Pct = (kv.Value * 100.0 / total).ToString("F1") }).OrderByDescending(x => x.Count).ToList(),
            BackStats = backMap.Select(kv => new NumberStatItem { Num = kv.Key.ToString("D2"), Count = kv.Value, Pct = (kv.Value * 100.0 / total).ToString("F1") }).OrderByDescending(x => x.Count).ToList()
        };
    }

    private HistoryListDto ToDto(SavedNumberHistory entity)
    {
        return new HistoryListDto
        {
            Id = entity.Id,
            IssueId = entity.IssueId,
            FrontNumbers = entity.FrontNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.Parse(s.Trim())).ToList(),
            BackNumbers = entity.BackNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.Parse(s.Trim())).ToList(),
            IsGenerated = entity.IsGenerated,
            MatchDetails = entity.MatchDetails,
            IterationCount = entity.IterationCount,
            CreatedAt = entity.CreatedAt
        };
    }
}
