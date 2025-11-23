using CurrencyExchange.Domain.Entities;

namespace CurrencyExchange.Domain.Interfaces;

/// <summary>
/// Repository interface for currency conversion operations
/// </summary>
public interface ICurrencyConversionRepository
{
    Task<CurrencyConversion> AddAsync(CurrencyConversion conversion, CancellationToken cancellationToken = default);
    Task<IEnumerable<CurrencyConversion>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CurrencyConversion>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, 
        CancellationToken cancellationToken = default);
    Task<IEnumerable<CurrencyConversion>> GetByFromCurrencyAsync(string currencyCode, 
        CancellationToken cancellationToken = default);
    Task<IEnumerable<CurrencyConversion>> GetFilteredAsync(string? fromCurrency = null, 
        DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}
