using Microsoft.AspNetCore.Mvc;
using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Dtos;
using ReverseNumbers.Api.Services;

namespace ReverseNumbers.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketController : ControllerBase
{
    private readonly ITicketService _svc;
    public TicketController(ITicketService svc) => _svc = svc;

    [HttpPost("{issueId}")]
    public async Task<ApiResult<object>> Save(int issueId, [FromBody] TicketDto dto)
    {
        // 多注：默认写入玩家号码；复式：仅保存票号（可在列表手动"转入玩家"）
        dto.AddToPlayer = string.Equals(dto.TicketType, "multi", StringComparison.OrdinalIgnoreCase);
        var ticket = await _svc.SaveAsync(issueId, dto);
        int count = 0;
        if (dto.AddToPlayer)
            count = await _svc.ExpandToPlayerNumbersAsync(ticket.Id, issueId, dto);
        return ApiResult<object>.Success(new { ticket, expandedCount = count },
            dto.AddToPlayer ? $"票号保存成功，已写入玩家号码 {count} 注" : "票号保存成功（未添加到玩家号码）");
    }

    [HttpGet("{issueId}")]
    public async Task<ApiResult<object>> GetByIssueId(int issueId)
        => ApiResult<object>.Success(await _svc.GetByIssueIdAsync(issueId));

    [HttpPut("{id}/constraints")]
    public async Task<ApiResult> UpdateConstraints(int id, [FromBody] List<TicketConstraintDto> constraints)
    {
        await _svc.UpdateConstraintsAsync(id, constraints);
        return ApiResult.Success("更新成功");
    }

    [HttpPost("{id}/to-player")]
    public async Task<ApiResult<object>> AddToPlayer(int id)
    {
        var count = await _svc.AddToPlayerAsync(id);
        return ApiResult<object>.Success(new { expandedCount = count }, $"转入玩家号码成功，展开为 {count} 注");
    }

    [HttpGet("analysis")]
    public async Task<ApiResult<object>> GetAnalysis()
        => ApiResult<object>.Success(await _svc.GetAnalysisAsync());

    [HttpDelete("{id}")]
    public async Task<ApiResult> Delete(int id)
    {
        await _svc.DeleteAsync(id);
        return ApiResult.Success("删除成功");
    }
}
