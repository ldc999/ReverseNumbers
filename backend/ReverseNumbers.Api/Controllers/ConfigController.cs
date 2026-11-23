using Microsoft.AspNetCore.Mvc;
using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Dtos;
using ReverseNumbers.Api.Services;

namespace ReverseNumbers.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IConfigService _service;

    public ConfigController(IConfigService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ApiResult<object>> GetAll()
    {
        var all = await _service.GetAllAsync();
        return ApiResult<object>.Success(all);
    }

    [HttpGet("{key}")]
    public async Task<ApiResult<object>> GetByKey(string key)
    {
        var value = await _service.GetValueAsync(key);
        return ApiResult<object>.Success(new { key, value });
    }

    [HttpPut("{key}")]
    public async Task<ApiResult> Update(string key, [FromBody] ConfigUpdateDto dto)
    {
        await _service.SetValueAsync(key, dto.Value);
        return ApiResult.Success("更新成功");
    }
}
