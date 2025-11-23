using CurrencyExchange.Domain.Entities;

namespace CurrencyExchange.Domain.Interfaces;

/// <summary>
/// Interface for external currency rate API service
/// </summary>
public interface IExternalCurrencyService
{
    Task<IEnumerable<CurrencyRate>> FetchLatestRatesAsync(CancellationToken cancellationToken = default);
}
