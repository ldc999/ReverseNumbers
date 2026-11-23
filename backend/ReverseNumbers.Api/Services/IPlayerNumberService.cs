using ReverseNumbers.Api.Models.Entities;

namespace ReverseNumbers.Api.Services;

public interface IPlayerNumberService
{
    Task<List<PlayerNumber>> GetByIssueIdAsync(int issueId);
    Task<PlayerNumber> AddAsync(int issueId, string frontNumbers, string backNumbers);
    Task<List<PlayerNumber>> BatchAddAsync(int issueId, List<(string Front, string Back)> sets);
    Task DeleteAsync(int issueId, int id);
    Task ClearAllAsync(int issueId);
}
