using SqlSugar;

namespace ReverseNumbers.Api.Models.Entities;

/// <summary>
/// 历史开奖号码缓存（数据来源：500.com）
/// </summary>
[SugarTable("lottery_draw")]
public class LotteryDraw
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>玩法：dlt / ssq</summary>
    [SugarColumn(ColumnName = "game", Length = 10, IsNullable = false)]
    public string Game { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "issue_number", Length = 20, IsNullable = false)]
    public string IssueNumber { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "front_numbers", Length = 50, IsNullable = false)]
    public string FrontNumbers { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "back_numbers", Length = 20, IsNullable = false)]
    public string BackNumbers { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "draw_date", Length = 20, IsNullable = true)]
    public string? DrawDate { get; set; }

    [SugarColumn(ColumnName = "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
