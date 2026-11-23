namespace ReverseNumbers.Api.Models.Dtos;

public class IssueDto
{
    public string IssueNumber { get; set; } = string.Empty;
}

public class DrawResultDto
{
    public string WinningNumbers1 { get; set; } = string.Empty;
    public string WinningNumbers2 { get; set; } = string.Empty;
}

public class PlayerNumberInputDto
{
    public string FrontNumbers { get; set; } = string.Empty;
    public string BackNumbers { get; set; } = string.Empty;
}

public class PlayerNumberBatchDto
{
    public List<PlayerNumberInputDto> Sets { get; set; } = new();
}

public class GenerateRequestDto
{
    public int IssueId { get; set; }
    public int MaxFrontHighlight { get; set; } = 3;
    public int MaxBackHighlight { get; set; } = 1;
    public List<TicketConstraintDto> HighlightConstraints { get; set; } = new();
    public List<int> ExcludedFrontNumbers { get; set; } = new();
    public List<int> ExcludedBackNumbers { get; set; } = new();
    public List<int> RequiredFrontNumbers { get; set; } = new();
    public List<int> RequiredBackNumbers { get; set; } = new();
    public List<TicketConstraintSetDto> TicketConstraints { get; set; } = new();
    public bool EnableHistoryConstraint { get; set; }
    public DateTime? HistoryStart { get; set; }
    public DateTime? HistoryEnd { get; set; }
    public List<TicketConstraintDto> HistoryConstraints { get; set; } = new();
}

public class TicketConstraintSetDto
{
    public List<int> FrontPool { get; set; } = new();
    public List<int> BackPool { get; set; } = new();
    public List<TicketConstraintDto> Constraints { get; set; } = new();
}

public class HistoryStatsDto
{
    public List<NumberStatItem> FrontStats { get; set; } = new();
    public List<NumberStatItem> BackStats { get; set; } = new();
    public int TotalSets { get; set; }
}

public class NumberStatItem
{
    public string Num { get; set; } = "";
    public int Count { get; set; }
    public string Pct { get; set; } = "0.0";
}

public class LotteryDrawDto
{
    public string IssueNumber { get; set; } = "";
    public List<int> FrontNumbers { get; set; } = new();
    public List<int> BackNumbers { get; set; } = new();
    public string? DrawDate { get; set; }
}

public class TrendResponseDto
{
    public string Game { get; set; } = "";
    public int FrontMax { get; set; }
    public int BackMax { get; set; }
    public List<LotteryDrawDto> Draws { get; set; } = new();
    /// <summary>本地缓存的总期数</summary>
    public int CachedCount { get; set; }
}

public class TicketAnalysisDto
{
    public int TicketId { get; set; }
    public string IssueNumber { get; set; } = "";
    public int IssueId { get; set; }
    public string TicketType { get; set; } = "";
    public int FrontPoolCount { get; set; }
    public int FrontMatchCount { get; set; }
    public int BackPoolCount { get; set; }
    public int BackMatchCount { get; set; }
}

public class HighlightRequestDto
{
    public int IssueId { get; set; }
    public List<int> FrontNumbers { get; set; } = new();
    public List<int> BackNumbers { get; set; } = new();
}

public class GenerateResponseDto
{
    public List<int> FrontNumbers { get; set; } = new();
    public List<int> BackNumbers { get; set; } = new();
    public Dictionary<string, List<int>> FrontMatchDetails { get; set; } = new();
    public Dictionary<string, List<int>> BackMatchDetails { get; set; } = new();
    public int IterationCount { get; set; }
}

public class HistorySaveDto
{
    public int IssueId { get; set; }
    public List<int> FrontNumbers { get; set; } = new();
    public List<int> BackNumbers { get; set; } = new();
    public bool IsGenerated { get; set; }
    public string? MatchDetails { get; set; }
    public int IterationCount { get; set; }
}

public class HistoryListDto
{
    public int Id { get; set; }
    public int IssueId { get; set; }
    public List<int> FrontNumbers { get; set; } = new();
    public List<int> BackNumbers { get; set; } = new();
    public bool IsGenerated { get; set; }
    public string? MatchDetails { get; set; }
    public int IterationCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ConfigUpdateDto
{
    public string Value { get; set; } = string.Empty;
}

public class NumberSaveDto
{
    public List<int> FrontNumbers { get; set; } = new();
    public List<int> BackNumbers { get; set; } = new();
}

public class RetainedNumberDto
{
    public int Id { get; set; }
    public int IssueId { get; set; }
    public List<int> FrontNumbers { get; set; } = new();
    public List<int> BackNumbers { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class FavoriteNumberDto
{
    public int Id { get; set; }
    public List<int> FrontNumbers { get; set; } = new();
    public List<int> BackNumbers { get; set; } = new();
    public string? Label { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TicketDto
{
    public List<int> FrontPool { get; set; } = new();
    public List<int> BackPool { get; set; } = new();
    public List<TicketConstraintDto> Constraints { get; set; } = new();
    public List<PlayerNumberInputDto> Bets { get; set; } = new();
    public string TicketType { get; set; } = "fushi";
    public bool AddToPlayer { get; set; } = true;
}

public class TicketConstraintDto
{
    public int F { get; set; }
    public int B { get; set; }
    // 下限，默认0（即只限制上限时的旧行为）
    public int FMin { get; set; }
    public int BMin { get; set; }
}

public class TicketListDto
{
    public int Id { get; set; }
    public int IssueId { get; set; }
    public List<int> FrontPool { get; set; } = new();
    public List<int> BackPool { get; set; } = new();
    public List<TicketConstraintDto> Constraints { get; set; } = new();
    public string TicketType { get; set; } = "fushi";
    public List<PlayerNumberInputDto> Bets { get; set; } = new();
    public bool AddToPlayer { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
