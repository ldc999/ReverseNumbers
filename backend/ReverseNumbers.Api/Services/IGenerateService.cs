using ReverseNumbers.Api.Models.Dtos;

namespace ReverseNumbers.Api.Services;

public interface IGenerateService
{
    Task<GenerateResponseDto> GenerateRandomAsync(GenerateRequestDto request);
    Task<GenerateResponseDto> HighlightOnlyAsync(HighlightRequestDto request);
}
