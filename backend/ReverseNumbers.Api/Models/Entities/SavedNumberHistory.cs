using SqlSugar;

namespace ReverseNumbers.Api.Models.Entities;

[SugarTable("saved_number_history")]
public class SavedNumberHistory
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(ColumnName = "issue_id", IsNullable = false)]
    public int IssueId { get; set; }

    [SugarColumn(ColumnName = "front_numbers", Length = 50, IsNullable = false)]
    public string FrontNumbers { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "back_numbers", Length = 20, IsNullable = false)]
    public string BackNumbers { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "is_generated")]
    public bool IsGenerated { get; set; }

    [SugarColumn(ColumnName = "match_details", ColumnDataType = "text", IsNullable = true)]
    public string? MatchDetails { get; set; }

    [SugarColumn(ColumnName = "iteration_count")]
    public int IterationCount { get; set; }

    [SugarColumn(ColumnName = "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
