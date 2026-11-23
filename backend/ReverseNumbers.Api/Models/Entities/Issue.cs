using SqlSugar;

namespace ReverseNumbers.Api.Models.Entities;

[SugarTable("issue")]
public class Issue
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(ColumnName = "issue_number", Length = 20, IsNullable = false)]
    public string IssueNumber { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "winning_front_numbers", Length = 50, IsNullable = true)]
    public string? WinningFrontNumbers { get; set; }

    [SugarColumn(ColumnName = "winning_back_numbers", Length = 20, IsNullable = true)]
    public string? WinningBackNumbers { get; set; }

    [SugarColumn(ColumnName = "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
