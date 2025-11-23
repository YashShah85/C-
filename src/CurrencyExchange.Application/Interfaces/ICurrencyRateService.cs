using CurrencyExchange.Application.DTOs;

namespace CurrencyExchange.Application.Interfaces;

/// <summary>
/// Service interface for currency rate operations
/// </summary>
public interface ICurrencyRateService
{
    Task<IEnumerable<CurrencyRateDto>> GetAllRatesAsync(CancellationToken cancellationToken = default);
    Task<CurrencyRateDto?> GetRateByCurrencyCodeAsync(string currencyCode, 
        CancellationToken cancellationToken = default);
    Task<int> UpdateRatesFromExternalSourceAsync(CancellationToken cancellationToken = default);
}
