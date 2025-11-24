using CurrencyExchange.Application.DTOs;
using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Domain.Entities;
using CurrencyExchange.Domain.Exceptions;
using CurrencyExchange.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CurrencyExchange.Application.Services;

/// <summary>
/// Service for handling currency rate operations
/// </summary>
public class CurrencyRateService : ICurrencyRateService
{
    private readonly ICurrencyRateRepository _rateRepository;
    private readonly IExternalCurrencyService _externalService;
    private readonly ILogger<CurrencyRateService> _logger;

    public CurrencyRateService(
        ICurrencyRateRepository rateRepository,
        IExternalCurrencyService externalService,
        ILogger<CurrencyRateService> logger)
    {
        _rateRepository = rateRepository ?? throw new ArgumentNullException(nameof(rateRepository));
        _externalService = externalService ?? throw new ArgumentNullException(nameof(externalService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<CurrencyRateDto>> GetAllRatesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all currency rates");

        var rates = await _rateRepository.GetAllAsync(cancellationToken);
        
        return rates.Select(MapToDto).ToList();
    }

    public async Task<CurrencyRateDto?> GetRateByCurrencyCodeAsync(
        string currencyCode, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching rate for currency: {CurrencyCode}", currencyCode);

        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new ArgumentException("Currency code cannot be null or empty", nameof(currencyCode));

        var rate = await _rateRepository.GetByCurrencyCodeAsync(
            currencyCode.ToUpperInvariant(), cancellationToken);

        return rate != null ? MapToDto(rate) : null;
    }

    public async Task<int> UpdateRatesFromExternalSourceAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting currency rates update from external source");

        try
        {
            var externalRates = await _externalService.FetchLatestRatesAsync(cancellationToken);
            
            if (externalRates == null || !externalRates.Any())
            {
                _logger.LogWarning("No rates fetched from external source");
                return 0;
            }

            var updatedCount = await _rateRepository.BulkUpsertAsync(externalRates, cancellationToken);
            
            _logger.LogInformation("Successfully updated {Count} currency rates", updatedCount);
            
            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating currency rates from external source");
            throw;
        }
    }

    private static CurrencyRateDto MapToDto(CurrencyRate rate)
    {
        return new CurrencyRateDto
        {
            CurrencyCode = rate.CurrencyCode,
            CurrencyName = rate.CurrencyName,
            RateToDKK = rate.RateToDKK,
            FetchedAt = rate.FetchedAt
        };
    }
}
