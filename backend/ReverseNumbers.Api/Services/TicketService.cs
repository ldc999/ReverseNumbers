using System.Text.Json;
using ReverseNumbers.Api.Common;
using ReverseNumbers.Api.Models.Dtos;
using ReverseNumbers.Api.Models.Entities;
using SqlSugar;

namespace ReverseNumbers.Api.Services;

public interface ITicketService
{
    Task<TicketListDto> SaveAsync(int issueId, TicketDto dto);
    Task<List<TicketListDto>> GetByIssueIdAsync(int issueId);
    Task DeleteAsync(int id);
    Task UpdateConstraintsAsync(int id, List<TicketConstraintDto> constraints);
    Task<int> ExpandToPlayerNumbersAsync(int ticketId, int issueId, TicketDto dto);
    Task<int> AddToPlayerAsync(int id);
    Task<List<TicketAnalysisDto>> GetAnalysisAsync();
}

public class TicketService : ITicketService
{
    private readonly ISqlSugarClient _db;
    public TicketService(ISqlSugarClient db) => _db = db;

    public async Task<TicketListDto> SaveAsync(int issueId, TicketDto dto)
    {
        var entity = new Ticket
        {
            IssueId = issueId,
            FrontPool = string.Join(",", dto.FrontPool.Select(n => n.ToString("D2"))),
            BackPool = string.Join(",", dto.BackPool.Select(n => n.ToString("D2"))),
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
    {
        var list = await _db.Queryable<Ticket>()
            .Where(t => t.IssueId == issueId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
        return list.Select(ToDto).ToList();
    }

    public async Task DeleteAsync(int id)
    {
        var ticket = await _db.Queryable<Ticket>().InSingleAsync(id)
            ?? throw new AppException("票号不存在");
        _db.Ado.BeginTran();
        try
        {
            // 若添加到玩家号码，级联删除关联的玩家号码
            if (ticket.AddToPlayer)
                await _db.Deleteable<PlayerNumber>().Where(p => p.TicketId == id).ExecuteCommandAsync();
            await _db.Deleteable<Ticket>().Where(t => t.Id == id).ExecuteCommandAsync();
            _db.Ado.CommitTran();
        }
        catch { _db.Ado.RollbackTran(); throw; }
    }

    public async Task UpdateConstraintsAsync(int id, List<TicketConstraintDto> constraints)
    {
        var ticket = await _db.Queryable<Ticket>().InSingleAsync(id)
            ?? throw new AppException("票号不存在");
        ticket.Constraints = JsonSerializer.Serialize(constraints);
        await _db.Updateable(ticket).ExecuteCommandAsync();
    }

    public async Task<int> ExpandToPlayerNumbersAsync(int ticketId, int issueId, TicketDto dto)
    {
        var ticket = await _db.Queryable<Ticket>().InSingleAsync(ticketId)
            ?? throw new AppException("票号不存在");

        var entities = new List<PlayerNumber>();

        if (dto.Bets != null && dto.Bets.Count > 0)
        {
            // 多注模式：直接保存每注
            foreach (var bet in dto.Bets)
            {
                entities.Add(new PlayerNumber
                {
                    IssueId = issueId,
                    TicketId = ticketId,
                    FrontNumbers = bet.FrontNumbers,
                    BackNumbers = bet.BackNumbers,
                    CreatedAt = DateTime.Now
                });
            }
        }
        else
        {
            // 复式模式：展开组合
            var frontPool = ticket.FrontPool.Split(',').Select(int.Parse).ToList();
            var backPool = ticket.BackPool.Split(',').Select(int.Parse).ToList();
            var frontCombs = Combinations(frontPool, 5);
            var backCombs = Combinations(backPool, 2);
            foreach (var fc in frontCombs)
                foreach (var bc in backCombs)
                    entities.Add(new PlayerNumber
                    {
                        IssueId = issueId,
                        TicketId = ticketId,
                        FrontNumbers = string.Join(",", fc.Select(n => n.ToString("D2"))),
                        BackNumbers = string.Join(",", bc.Select(n => n.ToString("D2"))),
                        CreatedAt = DateTime.Now
                    });
        }

        await _db.Insertable(entities).ExecuteCommandAsync();
        return entities.Count;
    }

    private static TicketListDto ToDto(Ticket t) => new()
    {
        Id = t.Id,
        IssueId = t.IssueId,
        FrontPool = t.FrontPool.Split(',').Select(int.Parse).ToList(),
        BackPool = t.BackPool.Split(',').Select(int.Parse).ToList(),
        Constraints = string.IsNullOrEmpty(t.Constraints) ? new() :
            JsonSerializer.Deserialize<List<TicketConstraintDto>>(t.Constraints) ?? new(),
        TicketType = t.TicketType,
        AddToPlayer = t.AddToPlayer,
        Bets = string.IsNullOrEmpty(t.BetsJson) ? new() :
            JsonSerializer.Deserialize<List<PlayerNumberInputDto>>(t.BetsJson) ?? new(),
        CreatedAt = t.CreatedAt
    };

    public async Task<int> AddToPlayerAsync(int id)
    {
        var ticket = await _db.Queryable<Ticket>().InSingleAsync(id)
            ?? throw new AppException("票号不存在");
        if (ticket.AddToPlayer) throw new AppException("该票号已添加到玩家号码");

        var dto = new TicketDto
        {
            FrontPool = ticket.FrontPool.Split(',').Select(int.Parse).ToList(),
            BackPool = ticket.BackPool.Split(',').Select(int.Parse).ToList(),
            TicketType = ticket.TicketType,
            Bets = string.IsNullOrEmpty(ticket.BetsJson) ? new() :
                JsonSerializer.Deserialize<List<PlayerNumberInputDto>>(ticket.BetsJson) ?? new()
        };

        var count = await ExpandToPlayerNumbersAsync(id, ticket.IssueId, dto);
        ticket.AddToPlayer = true;
        await _db.Updateable(ticket).ExecuteCommandAsync();
        return count;
    }

    public async Task<List<TicketAnalysisDto>> GetAnalysisAsync()
    {
        var tickets = await _db.Queryable<Ticket>().ToListAsync();
        var issues = await _db.Queryable<Issue>()
            .Where(i => i.WinningFrontNumbers != null || i.WinningBackNumbers != null)
            .ToListAsync();
        var issueMap = issues.ToDictionary(i => i.Id);

        var result = new List<TicketAnalysisDto>();
        foreach (var t in tickets)
        {
            if (!issueMap.TryGetValue(t.IssueId, out var issue)) continue;
            var fNums = t.FrontPool.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToHashSet();
            var bNums = t.BackPool.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToHashSet();
            var wf = (issue.WinningFrontNumbers ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToHashSet();
            var wb = (issue.WinningBackNumbers ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToHashSet();
            result.Add(new TicketAnalysisDto
            {
                TicketId = t.Id,
                TicketType = t.TicketType,
                IssueId = t.IssueId,
                IssueNumber = issue.IssueNumber,
                FrontPoolCount = fNums.Count,
                FrontMatchCount = fNums.Count(n => wf.Contains(n)),
                BackPoolCount = bNums.Count,
                BackMatchCount = bNums.Count(n => wb.Contains(n))
            });
        }
        return result.OrderByDescending(r => r.IssueNumber).ThenBy(r => r.IssueId).ToList();
    }

    private static List<List<int>> Combinations(List<int> arr, int k)
    {
        var result = new List<List<int>>();
        void Helper(int start, List<int> cur)
        {
            if (cur.Count == k) { result.Add(new List<int>(cur)); return; }
            for (int i = start; i < arr.Count; i++)
            { cur.Add(arr[i]); Helper(i + 1, cur); cur.RemoveAt(cur.Count - 1); }
        }
        Helper(0, new List<int>());
        return result;
    }
}
