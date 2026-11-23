using SqlSugar;

namespace ReverseNumbers.Api.Models.Entities;

[SugarTable("ssq_retained_number")]
public class SsqRetainedNumber
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(ColumnName = "issue_id", IsNullable = false)]
    public int IssueId { get; set; }

    [SugarColumn(ColumnName = "red_numbers", Length = 50, IsNullable = false)]
    public string RedNumbers { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "blue_numbers", Length = 20, IsNullable = false)]
    public string BlueNumbers { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
