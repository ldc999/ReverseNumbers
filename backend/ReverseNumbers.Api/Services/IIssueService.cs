using ReverseNumbers.Api.Models.Entities;

namespace ReverseNumbers.Api.Services;

public interface IIssueService
{
    Task<List<Issue>> GetAllAsync();
    Task<Issue?> GetByIdAsync(int id);
    Task<Issue> CreateAsync(string issueNumber);
    Task<Issue> UpdateAsync(int id, string issueNumber);
    Task DeleteAsync(int id);
    Task SetDrawResultAsync(int id, string winningFront, string winningBack);
}
