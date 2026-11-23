using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Entities;
using SqlSugar;

namespace ReverseNumbers.Api.Services;

public class IssueService : IIssueService
{
    private readonly ISqlSugarClient _db;

    public IssueService(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<List<Issue>> GetAllAsync()
    {
        return await _db.Queryable<Issue>()
            .OrderBy(i => i.IssueNumber, OrderByType.Desc)
            .ToListAsync();
    }

    public async Task<Issue?> GetByIdAsync(int id)
    {
        return await _db.Queryable<Issue>().InSingleAsync(id);
    }

    public async Task<Issue> CreateAsync(string issueNumber)
    {
        var exists = await _db.Queryable<Issue>()
            .Where(i => i.IssueNumber == issueNumber)
            .AnyAsync();
        if (exists)
            throw new AppException($"期号 {issueNumber} 已存在");

        var issue = new Issue { IssueNumber = issueNumber, CreatedAt = DateTime.Now };
        await _db.Insertable(issue).ExecuteCommandAsync();
        return issue;
    }

    public async Task<Issue> UpdateAsync(int id, string issueNumber)
    {
        var issue = await _db.Queryable<Issue>().InSingleAsync(id)
            ?? throw new AppException("期号不存在");

        var duplicate = await _db.Queryable<Issue>()
            .Where(i => i.IssueNumber == issueNumber && i.Id != id)
            .AnyAsync();
        if (duplicate)
            throw new AppException($"期号 {issueNumber} 已存在");

        issue.IssueNumber = issueNumber;
        await _db.Updateable(issue).ExecuteCommandAsync();
        return issue;
    }

    public async Task SetDrawResultAsync(int id, string winningFront, string winningBack)
    {
        var issue = await _db.Queryable<Issue>().InSingleAsync(id)
            ?? throw new AppException("期号不存在");
        issue.WinningFrontNumbers = string.IsNullOrWhiteSpace(winningFront) ? null : winningFront;
        issue.WinningBackNumbers = string.IsNullOrWhiteSpace(winningBack) ? null : winningBack;
        await _db.Updateable(issue).ExecuteCommandAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var issue = await _db.Queryable<Issue>().InSingleAsync(id)
            ?? throw new AppException("期号不存在");

        // 级联删除关联的玩家号码和历史记录
        _db.Ado.BeginTran();
        try
        {
            await _db.Deleteable<PlayerNumber>().Where(p => p.IssueId == id).ExecuteCommandAsync();
            await _db.Deleteable<SavedNumberHistory>().Where(h => h.IssueId == id).ExecuteCommandAsync();
            await _db.Deleteable<Issue>().Where(i => i.Id == id).ExecuteCommandAsync();
            _db.Ado.CommitTran();
        }
        catch
        {
            _db.Ado.RollbackTran();
            throw;
        }
    }
}
