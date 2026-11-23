using Microsoft.AspNetCore.Mvc;
using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Dtos;
using ReverseNumbers.Api.Services;

namespace ReverseNumbers.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly IHistoryService _service;

    public HistoryController(IHistoryService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ApiResult<object>> Save([FromBody] HistorySaveDto dto)
    {
        var result = await _service.SaveAsync(dto);
        return ApiResult<object>.Success(result, "保存成功");
    }

    [HttpGet("{issueId}")]
    public async Task<ApiResult<object>> GetByIssueId(int issueId)
    {
        var list = await _service.GetByIssueIdAsync(issueId);
        return ApiResult<object>.Success(list);
    }

    [HttpGet("detail/{id}")]
    public async Task<ApiResult<object>> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity == null)
            return ApiResult<object>.Fail("历史记录不存在");
        return ApiResult<object>.Success(entity);
    }

    [HttpGet("stats")]
    public async Task<ApiResult<object>> GetStats([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        var result = await _service.GetStatsAsync(start, end);
        return ApiResult<object>.Success(result);
    }

    [HttpDelete("{id}")]
    public async Task<ApiResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return ApiResult.Success("删除成功");
    }
}
