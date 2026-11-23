using SqlSugar;

namespace ReverseNumbers.Api.Models.Entities;

[SugarTable("retained_number")]
public class RetainedNumber
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(ColumnName = "issue_id", IsNullable = false)]
    public int IssueId { get; set; }

    [SugarColumn(ColumnName = "front_numbers", Length = 50, IsNullable = false)]
    public string FrontNumbers { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "back_numbers", Length = 20, IsNullable = false)]
    public string BackNumbers { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
