using SqlSugar;

namespace ReverseNumbers.Api.Models.Entities;

[SugarTable("favorite_number")]
public class FavoriteNumber
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(ColumnName = "front_numbers", Length = 50, IsNullable = false)]
    public string FrontNumbers { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "back_numbers", Length = 20, IsNullable = false)]
    public string BackNumbers { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "label", Length = 50, IsNullable = true)]
    public string? Label { get; set; }

    [SugarColumn(ColumnName = "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
