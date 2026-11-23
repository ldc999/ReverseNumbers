using System.Text.Json;
using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Dtos;
using ReverseNumbers.Api.Models.Entities;
using SqlSugar;

namespace ReverseNumbers.Api.Services.Ssq;

public interface ISsqTicketService
{
    Task<TicketListDto> SaveAsync(int issueId, TicketDto dto);
    Task<List<TicketListDto>> GetByIssueIdAsync(int issueId);
    Task DeleteAsync(int id);
    Task UpdateConstraintsAsync(int id, List<TicketConstraintDto> constraints);
    Task<int> ExpandToPlayerNumbersAsync(int ticketId, int issueId, TicketDto dto);
    Task<int> AddToPlayerAsync(int id);
    Task<List<TicketAnalysisDto>> GetAnalysisAsync();
}

public class SsqTicketService : ISsqTicketService
{
    private readonly ISqlSugarClient _db;
    public SsqTicketService(ISqlSugarClient db) => _db = db;

    public async Task<TicketListDto> SaveAsync(int issueId, TicketDto dto)
    {
        var entity = new SsqTicket
        {
            IssueId = issueId,
            RedPool = string.Join(",", dto.FrontPool.Select(n => n.ToString("D2"))),
            BluePool = string.Join(",", dto.BackPool.Select(n => n.ToString("D2"))),
            Constraints = JsonSerializer.Serialize(dto.Constraints),
            TicketType = dto.TicketType,
            AddToPlayer = dto.AddToPlayer,
            BetsJson = dto.Bets?.Count > 0 ? JsonSerializer.Serialize(dto.Bets) : null,
            CreatedAt = DateTime.Now
        };
        entity.Id = await _db.Insertable(entity).ExecuteReturnIdentityAsync();
        return ToDto(entity);
    }

    public async Task<List<TicketListDto>> GetByIssueIdAsync(int issueId)
        => (await _db.Queryable<SsqTicket>().Where(t => t.IssueId == issueId).OrderByDescending(t => t.CreatedAt).ToListAsync()).Select(ToDto).ToList();

    public async Task DeleteAsync(int id)
    {
        var ticket = await _db.Queryable<SsqTicket>().InSingleAsync(id)
            ?? throw new AppException("票号不存在");
        _db.Ado.BeginTran();
        try
        {
            if (ticket.AddToPlayer)
                await _db.Deleteable<SsqPlayerNumber>().Where(p => p.TicketId == id).ExecuteCommandAsync();
            await _db.Deleteable<SsqTicket>().Where(t => t.Id == id).ExecuteCommandAsync();
            _db.Ado.CommitTran();
        }
        catch { _db.Ado.RollbackTran(); throw; }
    }

    public async Task UpdateConstraintsAsync(int id, List<TicketConstraintDto> constraints)
    {
        var ticket = await _db.Queryable<SsqTicket>().InSingleAsync(id) ?? throw new AppException("票号不存在");
        ticket.Constraints = JsonSerializer.Serialize(constraints);
        await _db.Updateable(ticket).ExecuteCommandAsync();
    }

    public async Task<int> ExpandToPlayerNumbersAsync(int ticketId, int issueId, TicketDto dto)
    {
        var ticket = await _db.Queryable<SsqTicket>().InSingleAsync(ticketId) ?? throw new AppException("票号不存在");
        var entities = new List<SsqPlayerNumber>();
        if (dto.Bets != null && dto.Bets.Count > 0)
        {
            foreach (var bet in dto.Bets) entities.Add(new SsqPlayerNumber { IssueId = issueId, TicketId = ticketId, RedNumbers = bet.FrontNumbers, BlueNumbers = bet.BackNumbers, CreatedAt = DateTime.Now });
        }
        else
        {
            var rp = ticket.RedPool.Split(',').Select(int.Parse).ToList();
            var bp = ticket.BluePool.Split(',').Select(int.Parse).ToList();
            foreach (var rc in Comb(rp, 6)) foreach (var bc in Comb(bp, 1))
                    entities.Add(new SsqPlayerNumber { IssueId = issueId, TicketId = ticketId, RedNumbers = string.Join(",", rc.Select(n => n.ToString("D2"))), BlueNumbers = string.Join(",", bc.Select(n => n.ToString("D2"))), CreatedAt = DateTime.Now });
        }
        await _db.Insertable(entities).ExecuteCommandAsync();
        return entities.Count;
    }

    private static TicketListDto ToDto(SsqTicket t) => new()
    {
        Id = t.Id, IssueId = t.IssueId,
        FrontPool = t.RedPool.Split(',').Select(int.Parse).ToList(),
        BackPool = t.BluePool.Split(',').Select(int.Parse).ToList(),
        Constraints = string.IsNullOrEmpty(t.Constraints) ? new() : JsonSerializer.Deserialize<List<TicketConstraintDto>>(t.Constraints) ?? new(),
        TicketType = t.TicketType,
        AddToPlayer = t.AddToPlayer,
        Bets = string.IsNullOrEmpty(t.BetsJson) ? new() : JsonSerializer.Deserialize<List<PlayerNumberInputDto>>(t.BetsJson) ?? new(),
        CreatedAt = t.CreatedAt
    };

    public async Task<int> AddToPlayerAsync(int id)
    {
        var ticket = await _db.Queryable<SsqTicket>().InSingleAsync(id)
            ?? throw new AppException("票号不存在");
        if (ticket.AddToPlayer) throw new AppException("该票号已添加到玩家号码");
        var dto = new TicketDto
        {
            FrontPool = ticket.RedPool.Split(',').Select(int.Parse).ToList(),
            BackPool = ticket.BluePool.Split(',').Select(int.Parse).ToList(),
            TicketType = ticket.TicketType,
            Bets = string.IsNullOrEmpty(ticket.BetsJson) ? new() : JsonSerializer.Deserialize<List<PlayerNumberInputDto>>(ticket.BetsJson) ?? new()
        };
        var count = await ExpandToPlayerNumbersAsync(id, ticket.IssueId, dto);
        ticket.AddToPlayer = true;
        await _db.Updateable(ticket).ExecuteCommandAsync();
        return count;
    }

    public async Task<List<TicketAnalysisDto>> GetAnalysisAsync()
    {
        var tickets = await _db.Queryable<SsqTicket>().ToListAsync();
        var issues = await _db.Queryable<SsqIssue>()
            .Where(i => i.WinningRedNumbers != null || i.WinningBlueNumbers != null).ToListAsync();
        var issueMap = issues.ToDictionary(i => i.Id);
        var result = new List<TicketAnalysisDto>();
        foreach (var t in tickets)
        {
            if (!issueMap.TryGetValue(t.IssueId, out var issue)) continue;
            var fNums = t.RedPool.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToHashSet();
            var bNums = t.BluePool.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToHashSet();
            var wf = (issue.WinningRedNumbers ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToHashSet();
            var wb = (issue.WinningBlueNumbers ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToHashSet();
            result.Add(new TicketAnalysisDto
            {
                TicketId = t.Id, TicketType = t.TicketType, IssueId = t.IssueId, IssueNumber = issue.IssueNumber,
                FrontPoolCount = fNums.Count, FrontMatchCount = fNums.Count(n => wf.Contains(n)),
                BackPoolCount = bNums.Count, BackMatchCount = bNums.Count(n => wb.Contains(n))
            });
        }
        return result.OrderByDescending(r => r.IssueNumber).ToList();
    }

    private static List<List<int>> Comb(List<int> arr, int k) { var r = new List<List<int>>(); void H(int s, List<int> c) { if (c.Count == k) { r.Add(new List<int>(c)); return; } for (int i = s; i < arr.Count; i++) { c.Add(arr[i]); H(i + 1, c); c.RemoveAt(c.Count - 1); } } H(0, new List<int>()); return r; }
}
