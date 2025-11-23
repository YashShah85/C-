using CurrencyExchange.Application.DTOs;
using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Domain.Entities;
using CurrencyExchange.Domain.Exceptions;
using CurrencyExchange.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CurrencyExchange.Application.Services;

/// <summary>
/// Service for handling currency conversion operations
/// Follows Single Responsibility Principle (SRP) and Dependency Inversion Principle (DIP)
/// </summary>
public class CurrencyConversionService : ICurrencyConversionService
{
    private readonly ICurrencyRateRepository _rateRepository;
    private readonly ICurrencyConversionRepository _conversionRepository;
    private readonly ILogger<CurrencyConversionService> _logger;

    public CurrencyConversionService(
        ICurrencyRateRepository rateRepository,
        ICurrencyConversionRepository conversionRepository,
        ILogger<CurrencyConversionService> logger)
    {
        _rateRepository = rateRepository ?? throw new ArgumentNullException(nameof(rateRepository));
        _conversionRepository = conversionRepository ?? throw new ArgumentNullException(nameof(conversionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ConversionResponseDto> ConvertCurrencyAsync(
        ConversionRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Converting {Amount} {FromCurrency} to DKK", 
            request.Amount, 
            request.FromCurrency);

        // Validate input
        if (request.Amount <= 0)
            throw new InvalidConversionException("Amount must be greater than zero");

        var currencyCode = request.FromCurrency.ToUpperInvariant();

        // Special case: if converting from DKK to DKK
        if (currencyCode == "DKK")
        {
            var dkkConversion = new ConversionResponseDto
            {
                FromCurrency = "DKK",
                ToCurrency = "DKK",
                OriginalAmount = request.Amount,
                ConvertedAmount = request.Amount,
                ExchangeRate = 1.0m,
                ConversionDate = DateTime.UtcNow
            };

            // Still save the conversion
            await SaveConversionAsync(dkkConversion, cancellationToken);

            return dkkConversion;
        }

        // Get exchange rate from database
        var rate = await _rateRepository.GetByCurrencyCodeAsync(currencyCode, cancellationToken);

        if (rate == null)
            throw new CurrencyNotFoundException(currencyCode);

        // Calculate converted amount
        var convertedAmount = request.Amount * rate.RateToDKK;

        var response = new ConversionResponseDto
        {
            FromCurrency = rate.CurrencyCode,
            ToCurrency = "DKK",
            OriginalAmount = request.Amount,
            ConvertedAmount = Math.Round(convertedAmount, 2),
            ExchangeRate = rate.RateToDKK,
            ConversionDate = DateTime.UtcNow
        };

        // Save conversion to database
        await SaveConversionAsync(response, cancellationToken);

        _logger.LogInformation(
            "Successfully converted {Amount} {FromCurrency} to {ConvertedAmount} DKK", 
            request.Amount, 
            request.FromCurrency,
            response.ConvertedAmount);

        return response;
    }

    public async Task<IEnumerable<ConversionHistoryDto>> GetConversionHistoryAsync(
        string? fromCurrency = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Fetching conversion history - FromCurrency: {FromCurrency}, StartDate: {StartDate}, EndDate: {EndDate}",
            fromCurrency, startDate, endDate);

        var conversions = await _conversionRepository.GetFilteredAsync(
            fromCurrency?.ToUpperInvariant(), 
            startDate, 
            endDate, 
            cancellationToken);

        return conversions.Select(MapToHistoryDto).ToList();
    }

    private async Task SaveConversionAsync(
        ConversionResponseDto response, 
        CancellationToken cancellationToken)
    {
        var conversion = new CurrencyConversion(
            response.FromCurrency,
            response.ToCurrency,
            response.OriginalAmount,
            response.ConvertedAmount,
            response.ExchangeRate);

        await _conversionRepository.AddAsync(conversion, cancellationToken);
    }

    private static ConversionHistoryDto MapToHistoryDto(CurrencyConversion conversion)
    {
        return new ConversionHistoryDto
        {
            Id = conversion.Id,
            FromCurrency = conversion.FromCurrency,
            ToCurrency = conversion.ToCurrency,
            OriginalAmount = conversion.OriginalAmount,
            ConvertedAmount = conversion.ConvertedAmount,
            ExchangeRate = conversion.ExchangeRate,
            ConversionDate = conversion.ConversionDate
        };
    }
}
