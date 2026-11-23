using SqlSugar;

namespace ReverseNumbers.Api.Models.Entities;

[SugarTable("system_config")]
public class SystemConfig
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(ColumnName = "config_key", Length = 50, IsNullable = false)]
    public string ConfigKey { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "config_value", Length = 500, IsNullable = false)]
    public string ConfigValue { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "description", Length = 200, IsNullable = true)]
    public string? Description { get; set; }

    [SugarColumn(ColumnName = "updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
