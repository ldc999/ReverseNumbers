using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Entities;
using SqlSugar;

namespace ReverseNumbers.Api.Services.Ssq;

public interface ISsqIssueService
{
    Task<List<SsqIssue>> GetAllAsync();
    Task<SsqIssue?> GetByIdAsync(int id);
    Task<SsqIssue> CreateAsync(string issueNumber);
    Task<SsqIssue> UpdateAsync(int id, string issueNumber);
    Task DeleteAsync(int id);
    Task SetDrawResultAsync(int id, string winningRed, string winningBlue);
}

public class SsqIssueService : ISsqIssueService
{
    private readonly ISqlSugarClient _db;
    public SsqIssueService(ISqlSugarClient db) => _db = db;

    public async Task<List<SsqIssue>> GetAllAsync()
        => await _db.Queryable<SsqIssue>().OrderBy(i => i.IssueNumber, OrderByType.Desc).ToListAsync();

    public async Task<SsqIssue?> GetByIdAsync(int id)
        => await _db.Queryable<SsqIssue>().InSingleAsync(id);

    public async Task<SsqIssue> CreateAsync(string issueNumber)
    {
        if (await _db.Queryable<SsqIssue>().Where(i => i.IssueNumber == issueNumber).AnyAsync())
            throw new AppException($"期号 {issueNumber} 已存在");
        var issue = new SsqIssue { IssueNumber = issueNumber, CreatedAt = DateTime.Now };
        await _db.Insertable(issue).ExecuteCommandAsync();
        return issue;
    }

    public async Task<SsqIssue> UpdateAsync(int id, string issueNumber)
    {
        var issue = await _db.Queryable<SsqIssue>().InSingleAsync(id)
            ?? throw new AppException("期号不存在");
        if (await _db.Queryable<SsqIssue>().Where(i => i.IssueNumber == issueNumber && i.Id != id).AnyAsync())
            throw new AppException($"期号 {issueNumber} 已存在");
        issue.IssueNumber = issueNumber;
        await _db.Updateable(issue).ExecuteCommandAsync();
        return issue;
    }

    public async Task SetDrawResultAsync(int id, string winningRed, string winningBlue)
    {
        var issue = await _db.Queryable<SsqIssue>().InSingleAsync(id)
            ?? throw new AppException("期号不存在");
        issue.WinningRedNumbers = string.IsNullOrWhiteSpace(winningRed) ? null : winningRed;
        issue.WinningBlueNumbers = string.IsNullOrWhiteSpace(winningBlue) ? null : winningBlue;
        await _db.Updateable(issue).ExecuteCommandAsync();
    }

    public async Task DeleteAsync(int id)
    {
        _db.Ado.BeginTran();
        try
        {
            await _db.Deleteable<SsqPlayerNumber>().Where(p => p.IssueId == id).ExecuteCommandAsync();
            await _db.Deleteable<SsqSavedNumberHistory>().Where(h => h.IssueId == id).ExecuteCommandAsync();
            await _db.Deleteable<SsqRetainedNumber>().Where(r => r.IssueId == id).ExecuteCommandAsync();
            await _db.Deleteable<SsqIssue>().Where(i => i.Id == id).ExecuteCommandAsync();
            _db.Ado.CommitTran();
        }
        catch { _db.Ado.RollbackTran(); throw; }
    }
}
