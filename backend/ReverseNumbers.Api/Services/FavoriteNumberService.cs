using ReverseNumbers.Api.Models.Dtos;
using ReverseNumbers.Api.Models.Entities;
using SqlSugar;

namespace ReverseNumbers.Api.Services;

public class FavoriteNumberService : IFavoriteNumberService
{
    private readonly ISqlSugarClient _db;

    public FavoriteNumberService(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<FavoriteNumberDto> SaveAsync(NumberSaveDto dto, string? label)
    {
        var entity = new FavoriteNumber
        {
            FrontNumbers = string.Join(",", dto.FrontNumbers.Select(n => n.ToString("D2"))),
            BackNumbers = string.Join(",", dto.BackNumbers.Select(n => n.ToString("D2"))),
            Label = label,
            CreatedAt = DateTime.Now
        };
        await _db.Insertable(entity).ExecuteCommandAsync();
        return ToDto(entity);
    }

    public async Task<List<FavoriteNumberDto>> GetAllAsync()
    {
        var list = await _db.Queryable<FavoriteNumber>()
            .OrderBy(f => f.CreatedAt, OrderByType.Desc)
            .ToListAsync();
        return list.Select(ToDto).ToList();
    }

    public async Task DeleteAsync(int id)
    {
        await _db.Deleteable<FavoriteNumber>().Where(f => f.Id == id).ExecuteCommandAsync();
    }

    private static FavoriteNumberDto ToDto(FavoriteNumber entity)
    {
        return new FavoriteNumberDto
        {
            Id = entity.Id,
            FrontNumbers = entity.FrontNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse).ToList(),
            BackNumbers = entity.BackNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse).ToList(),
            Label = entity.Label,
            CreatedAt = entity.CreatedAt
        };
    }
}
