using ReverseNumbers.Api.Models.Dtos;

namespace ReverseNumbers.Api.Services;

public interface IFavoriteNumberService
{
    Task<FavoriteNumberDto> SaveAsync(NumberSaveDto dto, string? label);
    Task<List<FavoriteNumberDto>> GetAllAsync();
    Task DeleteAsync(int id);
}
