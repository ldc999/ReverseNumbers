using SqlSugar;

namespace ReverseNumbers.Api.Models.Entities;

[SugarTable("ssq_issue")]
public class SsqIssue
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(ColumnName = "issue_number", Length = 20, IsNullable = false)]
    public string IssueNumber { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "winning_red_numbers", Length = 50, IsNullable = true)]
    public string? WinningRedNumbers { get; set; }

    [SugarColumn(ColumnName = "winning_blue_numbers", Length = 20, IsNullable = true)]
    public string? WinningBlueNumbers { get; set; }

    [SugarColumn(ColumnName = "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
