using CurrencyExchange.Domain.Entities;

namespace CurrencyExchange.Domain.Interfaces;

/// <summary>
/// Repository interface for currency rate operations
/// </summary>
public interface ICurrencyRateRepository
{
    Task<CurrencyRate?> GetByCurrencyCodeAsync(string currencyCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<CurrencyRate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CurrencyRate> AddAsync(CurrencyRate currencyRate, CancellationToken cancellationToken = default);
    Task UpdateAsync(CurrencyRate currencyRate, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string currencyCode, CancellationToken cancellationToken = default);
    Task<int> BulkUpsertAsync(IEnumerable<CurrencyRate> currencyRates, CancellationToken cancellationToken = default);
}
