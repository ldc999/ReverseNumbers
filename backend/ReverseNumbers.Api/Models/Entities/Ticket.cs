using SqlSugar;

namespace ReverseNumbers.Api.Models.Entities;

[SugarTable("ticket")]
public class Ticket
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(ColumnName = "issue_id", IsNullable = false)]
    public int IssueId { get; set; }

    [SugarColumn(ColumnName = "front_pool", Length = 200, IsNullable = false)]
    public string FrontPool { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "back_pool", Length = 100, IsNullable = false)]
    public string BackPool { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "constraints", ColumnDataType = "text", IsNullable = true)]
    public string? Constraints { get; set; }

    [SugarColumn(ColumnName = "ticket_type", Length = 20, IsNullable = false)]
    public string TicketType { get; set; } = "fushi";

    [SugarColumn(ColumnName = "add_to_player")]
    public bool AddToPlayer { get; set; } = true;

    [SugarColumn(ColumnName = "bets_json", ColumnDataType = "text", IsNullable = true)]
    public string? BetsJson { get; set; }

    [SugarColumn(ColumnName = "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
