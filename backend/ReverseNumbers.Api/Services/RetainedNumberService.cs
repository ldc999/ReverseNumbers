using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Dtos;
using ReverseNumbers.Api.Models.Entities;
using SqlSugar;

namespace ReverseNumbers.Api.Services;

public class RetainedNumberService : IRetainedNumberService
{
    private readonly ISqlSugarClient _db;

    public RetainedNumberService(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<RetainedNumberDto> SaveAsync(int issueId, NumberSaveDto dto)
    {
        // Check issue exists
        var issue = await _db.Queryable<Issue>().InSingleAsync(issueId)
            ?? throw new AppException("期号不存在");

        var entity = new RetainedNumber
        {
            IssueId = issueId,
            FrontNumbers = string.Join(",", dto.FrontNumbers.Select(n => n.ToString("D2"))),
            BackNumbers = string.Join(",", dto.BackNumbers.Select(n => n.ToString("D2"))),
            CreatedAt = DateTime.Now
        };
        await _db.Insertable(entity).ExecuteCommandAsync();
        return ToDto(entity);
    }

    public async Task<List<RetainedNumberDto>> GetByIssueIdAsync(int issueId)
    {
        var list = await _db.Queryable<RetainedNumber>()
            .Where(r => r.IssueId == issueId)
            .OrderBy(r => r.CreatedAt, OrderByType.Desc)
            .ToListAsync();
        return list.Select(ToDto).ToList();
    }

    public async Task DeleteAsync(int id)
    {
        await _db.Deleteable<RetainedNumber>().Where(r => r.Id == id).ExecuteCommandAsync();
    }

    private static RetainedNumberDto ToDto(RetainedNumber entity)
    {
        return new RetainedNumberDto
        {
            Id = entity.Id,
            IssueId = entity.IssueId,
            FrontNumbers = entity.FrontNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse).ToList(),
            BackNumbers = entity.BackNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse).ToList(),
            CreatedAt = entity.CreatedAt
        };
    }
}
