using Microsoft.AspNetCore.Mvc;
using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Dtos;
using ReverseNumbers.Api.Services;

namespace ReverseNumbers.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IssueController : ControllerBase
{
    private readonly IIssueService _service;

    public IssueController(IIssueService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ApiResult<object>> GetAll()
    {
        var list = await _service.GetAllAsync();
        return ApiResult<object>.Success(list);
    }

    [HttpGet("{id}")]
    public async Task<ApiResult<object>> GetById(int id)
    {
        var issue = await _service.GetByIdAsync(id);
        if (issue == null)
            return ApiResult<object>.Fail("期号不存在");
        return ApiResult<object>.Success(issue);
    }

    [HttpPost]
    public async Task<ApiResult<object>> Create([FromBody] IssueDto dto)
    {
        var issue = await _service.CreateAsync(dto.IssueNumber);
        return ApiResult<object>.Success(issue, "创建成功");
    }

    [HttpPut("{id}")]
    public async Task<ApiResult<object>> Update(int id, [FromBody] IssueDto dto)
    {
        var issue = await _service.UpdateAsync(id, dto.IssueNumber);
        return ApiResult<object>.Success(issue, "更新成功");
    }

    [HttpPut("{id}/result")]
    public async Task<ApiResult> SetDrawResult(int id, [FromBody] DrawResultDto dto)
    {
        await _service.SetDrawResultAsync(id, dto.WinningNumbers1, dto.WinningNumbers2);
        return ApiResult.Success("开奖结果保存成功");
    }

    [HttpDelete("{id}")]
    public async Task<ApiResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return ApiResult.Success("删除成功");
    }
}
