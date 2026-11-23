using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Entities;
using SqlSugar;

namespace ReverseNumbers.Api.Services;

public class PlayerNumberService : IPlayerNumberService
{
    private readonly ISqlSugarClient _db;

    public PlayerNumberService(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<List<PlayerNumber>> GetByIssueIdAsync(int issueId)
    {
        return await _db.Queryable<PlayerNumber>()
            .Where(p => p.IssueId == issueId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<PlayerNumber> AddAsync(int issueId, string frontNumbers, string backNumbers)
    {
        var issue = await _db.Queryable<Issue>().InSingleAsync(issueId)
            ?? throw new AppException("期号不存在");

        ValidateNumbers(frontNumbers, backNumbers);

        var entity = new PlayerNumber
        {
            IssueId = issueId,
            FrontNumbers = frontNumbers,
            BackNumbers = backNumbers,
            CreatedAt = DateTime.Now
        };
        await _db.Insertable(entity).ExecuteCommandAsync();
        return entity;
    }

    public async Task<List<PlayerNumber>> BatchAddAsync(int issueId, List<(string Front, string Back)> sets)
    {
        var issue = await _db.Queryable<Issue>().InSingleAsync(issueId)
            ?? throw new AppException("期号不存在");

        var entities = new List<PlayerNumber>();
        foreach (var (front, back) in sets)
        {
            ValidateNumbers(front, back);
            entities.Add(new PlayerNumber
            {
                IssueId = issueId,
                FrontNumbers = front,
                BackNumbers = back,
                CreatedAt = DateTime.Now
            });
        }

        await _db.Insertable(entities).ExecuteCommandAsync();
        return entities;
    }

    public async Task DeleteAsync(int issueId, int id)
    {
        var entity = await _db.Queryable<PlayerNumber>()
            .Where(p => p.IssueId == issueId && p.Id == id)
            .FirstAsync()
            ?? throw new AppException("号码记录不存在");

        await _db.Deleteable<PlayerNumber>().Where(p => p.Id == id).ExecuteCommandAsync();
    }

    public async Task ClearAllAsync(int issueId)
    {
        await _db.Deleteable<PlayerNumber>()
            .Where(p => p.IssueId == issueId)
            .ExecuteCommandAsync();
    }

    private void ValidateNumbers(string frontNumbers, string backNumbers)
    {
        var frontParts = frontNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var backParts = backNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries);

        if (frontParts.Length != 5)
            throw new AppException("前区必须为5个号码");
        if (backParts.Length != 2)
            throw new AppException("后区必须为2个号码");

        var frontNums = frontParts.Select(p => int.TryParse(p.Trim(), out var n) ? n : -1).ToList();
        var backNums = backParts.Select(p => int.TryParse(p.Trim(), out var n) ? n : -1).ToList();

        if (frontNums.Any(n => n < 1 || n > 35))
            throw new AppException("前区号码必须在01-35之间");
        if (backNums.Any(n => n < 1 || n > 12))
            throw new AppException("后区号码必须在01-12之间");
        if (frontNums.Distinct().Count() != 5)
            throw new AppException("前区号码不能重复");
        if (backNums.Distinct().Count() != 2)
            throw new AppException("后区号码不能重复");
    }
}
