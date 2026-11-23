using Microsoft.AspNetCore.Mvc;
using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Dtos;
using ReverseNumbers.Api.Services;

namespace ReverseNumbers.Api.Controllers;

[ApiController]
[Route("api/issue/{issueId}/player-numbers")]
public class PlayerNumberController : ControllerBase
{
    private readonly IPlayerNumberService _service;

    public PlayerNumberController(IPlayerNumberService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ApiResult<object>> GetByIssueId(int issueId)
    {
        var list = await _service.GetByIssueIdAsync(issueId);
        return ApiResult<object>.Success(list);
    }

    [HttpPost]
    public async Task<ApiResult<object>> Add(int issueId, [FromBody] PlayerNumberInputDto dto)
    {
        var entity = await _service.AddAsync(issueId, dto.FrontNumbers, dto.BackNumbers);
        return ApiResult<object>.Success(entity, "添加成功");
    }

    [HttpPost("batch")]
    public async Task<ApiResult<object>> BatchAdd(int issueId, [FromBody] PlayerNumberBatchDto dto)
    {
        var sets = dto.Sets.Select(s => (s.FrontNumbers, s.BackNumbers)).ToList();
        var entities = await _service.BatchAddAsync(issueId, sets);
        return ApiResult<object>.Success(entities, $"成功添加 {entities.Count} 注号码");
    }

    [HttpDelete("{id}")]
    public async Task<ApiResult> Delete(int issueId, int id)
    {
        await _service.DeleteAsync(issueId, id);
        return ApiResult.Success("删除成功");
    }

    [HttpDelete]
    public async Task<ApiResult> ClearAll(int issueId)
    {
        await _service.ClearAllAsync(issueId);
        return ApiResult.Success("清空成功");
    }
}
