using SqlSugar;

namespace ReverseNumbers.Api.Models.Entities;

[SugarTable("ssq_favorite_number")]
public class SsqFavoriteNumber
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(ColumnName = "red_numbers", Length = 50, IsNullable = false)]
    public string RedNumbers { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "blue_numbers", Length = 20, IsNullable = false)]
    public string BlueNumbers { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "label", Length = 50, IsNullable = true)]
    public string? Label { get; set; }

    [SugarColumn(ColumnName = "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
