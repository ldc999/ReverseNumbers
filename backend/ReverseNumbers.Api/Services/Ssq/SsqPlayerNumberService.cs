using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Entities;
using SqlSugar;

namespace ReverseNumbers.Api.Services.Ssq;

public interface ISsqPlayerNumberService
{
    Task<List<SsqPlayerNumber>> GetByIssueIdAsync(int issueId);
    Task<SsqPlayerNumber> AddAsync(int issueId, string redNumbers, string blueNumbers);
    Task<List<SsqPlayerNumber>> BatchAddAsync(int issueId, List<(string Red, string Blue)> sets);
    Task DeleteAsync(int issueId, int id);
    Task ClearAllAsync(int issueId);
}

public class SsqPlayerNumberService : ISsqPlayerNumberService
{
    private readonly ISqlSugarClient _db;
    public SsqPlayerNumberService(ISqlSugarClient db) => _db = db;

    public async Task<List<SsqPlayerNumber>> GetByIssueIdAsync(int issueId)
        => await _db.Queryable<SsqPlayerNumber>().Where(p => p.IssueId == issueId).OrderByDescending(p => p.CreatedAt).ToListAsync();

    public async Task<SsqPlayerNumber> AddAsync(int issueId, string redNumbers, string blueNumbers)
    {
        if (!await _db.Queryable<SsqIssue>().Where(i => i.Id == issueId).AnyAsync())
            throw new AppException("期号不存在");
        ValidateNumbers(redNumbers, blueNumbers);
        var entity = new SsqPlayerNumber { IssueId = issueId, RedNumbers = redNumbers, BlueNumbers = blueNumbers, CreatedAt = DateTime.Now };
        await _db.Insertable(entity).ExecuteCommandAsync();
        return entity;
    }

    public async Task<List<SsqPlayerNumber>> BatchAddAsync(int issueId, List<(string Red, string Blue)> sets)
    {
        if (!await _db.Queryable<SsqIssue>().Where(i => i.Id == issueId).AnyAsync())
            throw new AppException("期号不存在");
        var entities = new List<SsqPlayerNumber>();
        foreach (var (red, blue) in sets)
        {
            ValidateNumbers(red, blue);
            entities.Add(new SsqPlayerNumber { IssueId = issueId, RedNumbers = red, BlueNumbers = blue, CreatedAt = DateTime.Now });
        }
        await _db.Insertable(entities).ExecuteCommandAsync();
        return entities;
    }

    public async Task DeleteAsync(int issueId, int id)
    {
        var e = await _db.Queryable<SsqPlayerNumber>().Where(p => p.IssueId == issueId && p.Id == id).FirstAsync()
            ?? throw new AppException("号码记录不存在");
        await _db.Deleteable<SsqPlayerNumber>().Where(p => p.Id == id).ExecuteCommandAsync();
    }

    public async Task ClearAllAsync(int issueId)
        => await _db.Deleteable<SsqPlayerNumber>().Where(p => p.IssueId == issueId).ExecuteCommandAsync();

    private void ValidateNumbers(string red, string blue)
    {
        var redParts = red.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var blueParts = blue.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (redParts.Length != 6) throw new AppException("红球必须为6个号码");
        if (blueParts.Length != 1) throw new AppException("蓝球必须为1个号码");
        var redNums = redParts.Select(p => int.TryParse(p.Trim(), out var n) ? n : -1).ToList();
        var blueNums = blueParts.Select(p => int.TryParse(p.Trim(), out var n) ? n : -1).ToList();
        if (redNums.Any(n => n < 1 || n > 33)) throw new AppException("红球号码必须在01-33之间");
        if (blueNums.Any(n => n < 1 || n > 16)) throw new AppException("蓝球号码必须在01-16之间");
        if (redNums.Distinct().Count() != 6) throw new AppException("红球号码不能重复");
    }
}
