using ReverseNumbers.Api.Models.Dtos;

namespace ReverseNumbers.Api.Services;

public interface IRetainedNumberService
{
    Task<RetainedNumberDto> SaveAsync(int issueId, NumberSaveDto dto);
    Task<List<RetainedNumberDto>> GetByIssueIdAsync(int issueId);
    Task DeleteAsync(int id);
}
