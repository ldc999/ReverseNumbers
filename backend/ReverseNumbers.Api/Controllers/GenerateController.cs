using Microsoft.AspNetCore.Mvc;
using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Dtos;
using ReverseNumbers.Api.Services;

namespace ReverseNumbers.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenerateController : ControllerBase
{
    private readonly IGenerateService _service;

    public GenerateController(IGenerateService service)
    {
        _service = service;
    }

    [HttpPost("random")]
    public async Task<ApiResult<object>> GenerateRandom([FromBody] GenerateRequestDto dto)
    {
        var result = await _service.GenerateRandomAsync(dto);
        return ApiResult<object>.Success(result);
    }

    [HttpPost("highlight")]
    public async Task<ApiResult<object>> Highlight([FromBody] HighlightRequestDto dto)
    {
        var result = await _service.HighlightOnlyAsync(dto);
        return ApiResult<object>.Success(result);
    }
}
