using Microsoft.AspNetCore.Mvc;
using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Dtos;
using ReverseNumbers.Api.Services.Ssq;

namespace ReverseNumbers.Api.Controllers.Ssq;

// ── Issue ──
[ApiController, Route("api/ssq/issue")]
public class SsqIssueController : ControllerBase
{
    private readonly ISsqIssueService _svc;
    public SsqIssueController(ISsqIssueService svc) => _svc = svc;

    [HttpGet] public async Task<ApiResult<object>> GetAll() => ApiResult<object>.Success(await _svc.GetAllAsync());
    [HttpGet("{id}")] public async Task<ApiResult<object>> GetById(int id) { var i = await _svc.GetByIdAsync(id); return i == null ? ApiResult<object>.Fail("期号不存在") : ApiResult<object>.Success(i); }
    [HttpPost] public async Task<ApiResult<object>> Create([FromBody] IssueDto dto) => ApiResult<object>.Success(await _svc.CreateAsync(dto.IssueNumber), "创建成功");
    [HttpPut("{id}")] public async Task<ApiResult<object>> Update(int id, [FromBody] IssueDto dto) => ApiResult<object>.Success(await _svc.UpdateAsync(id, dto.IssueNumber), "更新成功");
    [HttpPut("{id}/result")] public async Task<ApiResult> SetDrawResult(int id, [FromBody] DrawResultDto dto) { await _svc.SetDrawResultAsync(id, dto.WinningNumbers1, dto.WinningNumbers2); return ApiResult.Success("开奖结果保存成功"); }
    [HttpDelete("{id}")] public async Task<ApiResult> Delete(int id) { await _svc.DeleteAsync(id); return ApiResult.Success("删除成功"); }
}

// ── PlayerNumber ──
[ApiController, Route("api/ssq/issue/{issueId}/player-numbers")]
public class SsqPlayerNumberController : ControllerBase
{
    private readonly ISsqPlayerNumberService _svc;
    public SsqPlayerNumberController(ISsqPlayerNumberService svc) => _svc = svc;

    [HttpGet] public async Task<ApiResult<object>> GetByIssueId(int issueId) => ApiResult<object>.Success(await _svc.GetByIssueIdAsync(issueId));
    [HttpPost] public async Task<ApiResult<object>> Add(int issueId, [FromBody] PlayerNumberInputDto dto) => ApiResult<object>.Success(await _svc.AddAsync(issueId, dto.FrontNumbers, dto.BackNumbers), "添加成功");
    [HttpPost("batch")] public async Task<ApiResult<object>> BatchAdd(int issueId, [FromBody] PlayerNumberBatchDto dto) { var sets = dto.Sets.Select(s => (s.FrontNumbers, s.BackNumbers)).ToList(); var r = await _svc.BatchAddAsync(issueId, sets); return ApiResult<object>.Success(r, $"成功添加 {r.Count} 注号码"); }
    [HttpDelete("{id}")] public async Task<ApiResult> Delete(int issueId, int id) { await _svc.DeleteAsync(issueId, id); return ApiResult.Success("删除成功"); }
    [HttpDelete] public async Task<ApiResult> ClearAll(int issueId) { await _svc.ClearAllAsync(issueId); return ApiResult.Success("清空成功"); }
}

// ── Generate ──
[ApiController, Route("api/ssq/generate")]
public class SsqGenerateController : ControllerBase
{
    private readonly ISsqGenerateService _svc;
    public SsqGenerateController(ISsqGenerateService svc) => _svc = svc;

    [HttpPost("random")] public async Task<ApiResult<object>> GenerateRandom([FromBody] GenerateRequestDto dto) => ApiResult<object>.Success(await _svc.GenerateRandomAsync(dto));
    [HttpPost("highlight")] public async Task<ApiResult<object>> Highlight([FromBody] HighlightRequestDto dto) => ApiResult<object>.Success(await _svc.HighlightOnlyAsync(dto));
}

// ── History ──
[ApiController, Route("api/ssq/history")]
public class SsqHistoryController : ControllerBase
{
    private readonly ISsqHistoryService _svc;
    public SsqHistoryController(ISsqHistoryService svc) => _svc = svc;

    [HttpPost] public async Task<ApiResult<object>> Save([FromBody] HistorySaveDto dto) => ApiResult<object>.Success(await _svc.SaveAsync(dto), "保存成功");
    [HttpGet("{issueId}")] public async Task<ApiResult<object>> GetByIssueId(int issueId) => ApiResult<object>.Success(await _svc.GetByIssueIdAsync(issueId));
    [HttpGet("detail/{id}")] public async Task<ApiResult<object>> GetById(int id) { var e = await _svc.GetByIdAsync(id); return e == null ? ApiResult<object>.Fail("不存在") : ApiResult<object>.Success(e); }
    [HttpGet("stats")] public async Task<ApiResult<object>> GetStats([FromQuery] DateTime start, [FromQuery] DateTime end) { var r = await _svc.GetStatsAsync(start, end); return ApiResult<object>.Success(r); }
    [HttpDelete("{id}")] public async Task<ApiResult> Delete(int id) { await _svc.DeleteAsync(id); return ApiResult.Success("删除成功"); }
}

// ── Config ──
[ApiController, Route("api/ssq/config")]
public class SsqConfigController : ControllerBase
{
    private readonly ISsqConfigService _svc;
    public SsqConfigController(ISsqConfigService svc) => _svc = svc;

    [HttpGet] public async Task<ApiResult<object>> GetAll() => ApiResult<object>.Success(await _svc.GetAllAsync());
    [HttpGet("{key}")] public async Task<ApiResult<object>> GetByKey(string key) => ApiResult<object>.Success(new { key, value = await _svc.GetValueAsync(key) });
    [HttpPut("{key}")] public async Task<ApiResult> Update(string key, [FromBody] ConfigUpdateDto dto) { await _svc.SetValueAsync(key, dto.Value); return ApiResult.Success("更新成功"); }
}

// ── RetainedNumber ──
[ApiController, Route("api/ssq/retained-number")]
public class SsqRetainedNumberController : ControllerBase
{
    private readonly ISsqRetainedNumberService _svc;
    public SsqRetainedNumberController(ISsqRetainedNumberService svc) => _svc = svc;

    [HttpPost("{issueId}")] public async Task<ApiResult<object>> Save(int issueId, [FromBody] NumberSaveDto dto) => ApiResult<object>.Success(await _svc.SaveAsync(issueId, dto), "保留成功");
    [HttpGet("{issueId}")] public async Task<ApiResult<object>> GetByIssueId(int issueId) => ApiResult<object>.Success(await _svc.GetByIssueIdAsync(issueId));
    [HttpDelete("{id}")] public async Task<ApiResult> Delete(int id) { await _svc.DeleteAsync(id); return ApiResult.Success("删除成功"); }
}

// ── Ticket ──
[ApiController, Route("api/ssq/ticket")]
public class SsqTicketController : ControllerBase
{
    private readonly ISsqTicketService _svc;
    public SsqTicketController(ISsqTicketService svc) => _svc = svc;
    [HttpPost("{issueId}")] public async Task<ApiResult<object>> Save(int issueId, [FromBody] TicketDto dto) { dto.AddToPlayer = string.Equals(dto.TicketType, "multi", StringComparison.OrdinalIgnoreCase); var t = await _svc.SaveAsync(issueId, dto); int c = 0; if (dto.AddToPlayer) c = await _svc.ExpandToPlayerNumbersAsync(t.Id, issueId, dto); return ApiResult<object>.Success(new { ticket = t, expandedCount = c }, dto.AddToPlayer ? $"票号保存成功，已写入玩家号码 {c} 注" : "票号保存成功（未添加到玩家号码）"); }
    [HttpGet("{issueId}")] public async Task<ApiResult<object>> GetByIssueId(int issueId) => ApiResult<object>.Success(await _svc.GetByIssueIdAsync(issueId));
    [HttpPut("{id}/constraints")] public async Task<ApiResult> UpdateConstraints(int id, [FromBody] List<TicketConstraintDto> constraints) { await _svc.UpdateConstraintsAsync(id, constraints); return ApiResult.Success("更新成功"); }
    [HttpPost("{id}/to-player")] public async Task<ApiResult<object>> AddToPlayer(int id) { var c = await _svc.AddToPlayerAsync(id); return ApiResult<object>.Success(new { expandedCount = c }, $"转入玩家号码成功，展开为 {c} 注"); }
    [HttpGet("analysis")] public async Task<ApiResult<object>> GetAnalysis() => ApiResult<object>.Success(await _svc.GetAnalysisAsync());
    [HttpDelete("{id}")] public async Task<ApiResult> Delete(int id) { await _svc.DeleteAsync(id); return ApiResult.Success("删除成功"); }
}

// ── FavoriteNumber ──
[ApiController, Route("api/ssq/favorite-number")]
public class SsqFavoriteNumberController : ControllerBase
{
    private readonly ISsqFavoriteNumberService _svc;
    public SsqFavoriteNumberController(ISsqFavoriteNumberService svc) => _svc = svc;

    [HttpPost] public async Task<ApiResult<object>> Save([FromBody] NumberSaveDto dto, [FromQuery] string? label = null) => ApiResult<object>.Success(await _svc.SaveAsync(dto, label), "收藏成功");
    [HttpGet] public async Task<ApiResult<object>> GetAll() => ApiResult<object>.Success(await _svc.GetAllAsync());
    [HttpDelete("{id}")] public async Task<ApiResult> Delete(int id) { await _svc.DeleteAsync(id); return ApiResult.Success("删除成功"); }
}
