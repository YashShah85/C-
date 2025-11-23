using CurrencyExchange.Application.DTOs;

namespace CurrencyExchange.Application.Interfaces;

/// <summary>
/// Service interface for currency conversion operations
/// </summary>
public interface ICurrencyConversionService
{
    Task<ConversionResponseDto> ConvertCurrencyAsync(ConversionRequestDto request, 
        CancellationToken cancellationToken = default);
    Task<IEnumerable<ConversionHistoryDto>> GetConversionHistoryAsync(
        string? fromCurrency = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        CancellationToken cancellationToken = default);
}
