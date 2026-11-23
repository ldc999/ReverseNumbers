using Microsoft.AspNetCore.Mvc;
using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Dtos;
using ReverseNumbers.Api.Services;

namespace ReverseNumbers.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RetainedNumberController : ControllerBase
{
    private readonly IRetainedNumberService _service;

    public RetainedNumberController(IRetainedNumberService service)
    {
        _service = service;
    }

    [HttpPost("{issueId}")]
    public async Task<ApiResult<object>> Save(int issueId, [FromBody] NumberSaveDto dto)
    {
        var result = await _service.SaveAsync(issueId, dto);
        return ApiResult<object>.Success(result, "保留成功");
    }

    [HttpGet("{issueId}")]
    public async Task<ApiResult<object>> GetByIssueId(int issueId)
    {
        var list = await _service.GetByIssueIdAsync(issueId);
        return ApiResult<object>.Success(list);
    }

    [HttpDelete("{id}")]
    public async Task<ApiResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return ApiResult.Success("删除成功");
    }
}
