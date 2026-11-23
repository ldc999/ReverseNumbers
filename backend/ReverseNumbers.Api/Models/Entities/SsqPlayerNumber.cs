using SqlSugar;

namespace ReverseNumbers.Api.Models.Entities;

[SugarTable("ssq_player_number")]
public class SsqPlayerNumber
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(ColumnName = "issue_id", IsNullable = false)]
    public int IssueId { get; set; }

    [SugarColumn(ColumnName = "ticket_id", IsNullable = true)]
    public int? TicketId { get; set; }

    [SugarColumn(ColumnName = "red_numbers", Length = 50, IsNullable = false)]
    public string RedNumbers { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "blue_numbers", Length = 20, IsNullable = false)]
    public string BlueNumbers { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
