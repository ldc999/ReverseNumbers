using Microsoft.AspNetCore.Mvc;
using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Dtos;
using ReverseNumbers.Api.Services;

namespace ReverseNumbers.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FavoriteNumberController : ControllerBase
{
    private readonly IFavoriteNumberService _service;

    public FavoriteNumberController(IFavoriteNumberService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ApiResult<object>> Save([FromBody] NumberSaveDto dto, [FromQuery] string? label = null)
    {
        var result = await _service.SaveAsync(dto, label);
        return ApiResult<object>.Success(result, "收藏成功");
    }

    [HttpGet]
    public async Task<ApiResult<object>> GetAll()
    {
        var list = await _service.GetAllAsync();
        return ApiResult<object>.Success(list);
    }

    [HttpDelete("{id}")]
    public async Task<ApiResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return ApiResult.Success("删除成功");
    }
}
