using Microsoft.AspNetCore.Mvc;
using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Services;

namespace ReverseNumbers.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LotteryController : ControllerBase
{
    private readonly ILotteryTrendService _svc;
    public LotteryController(ILotteryTrendService svc) => _svc = svc;

    /// <summary>获取历史开奖走势数据</summary>
    /// <param name="game">dlt / ssq</param>
    /// <param name="count">期数，最大300</param>
    /// <param name="refresh">true 时强制重新抓取 500.com</param>
    [HttpGet("trend")]
    public async Task<ApiResult<object>> GetTrend(
        [FromQuery] string game = "dlt",
        [FromQuery] int count = 100,
        [FromQuery] bool refresh = false)
    {
        var result = await _svc.GetTrendAsync(game, count, refresh);
        return ApiResult<object>.Success(result);
    }
}
