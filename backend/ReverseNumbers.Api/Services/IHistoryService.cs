using ReverseNumbers.Api.Models.Dtos;

namespace ReverseNumbers.Api.Services;

public interface IHistoryService
{
    Task<HistoryListDto> SaveAsync(HistorySaveDto dto);
    Task<List<HistoryListDto>> GetByIssueIdAsync(int issueId);
    Task<HistoryListDto?> GetByIdAsync(int id);
    Task DeleteAsync(int id);
    Task<HistoryStatsDto> GetStatsAsync(DateTime start, DateTime end);
}
