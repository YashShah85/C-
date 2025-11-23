using CurrencyExchange.Domain.Entities;
using CurrencyExchange.Domain.Interfaces;
using CurrencyExchange.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CurrencyExchange.Infrastructure.Repositories;

/// <summary>
/// SQL Server repository implementation for CurrencyConversion
/// Follows Repository Pattern
/// </summary>
public class CurrencyConversionRepository : ICurrencyConversionRepository
{
    private readonly CurrencyDbContext _context;
    private readonly ILogger<CurrencyConversionRepository> _logger;

    public CurrencyConversionRepository(
        CurrencyDbContext context,
        ILogger<CurrencyConversionRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CurrencyConversion> AddAsync(
        CurrencyConversion conversion, 
        CancellationToken cancellationToken = default)
    {
        _context.CurrencyConversions.Add(conversion);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Saved conversion: {Id}", conversion.Id);
        
        return conversion;
    }

    public async Task<IEnumerable<CurrencyConversion>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyConversions
            .AsNoTracking()
            .OrderByDescending(c => c.ConversionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CurrencyConversion>> GetByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyConversions
            .AsNoTracking()
            .Where(c => c.ConversionDate >= startDate && c.ConversionDate <= endDate)
            .OrderByDescending(c => c.ConversionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CurrencyConversion>> GetByFromCurrencyAsync(
        string currencyCode, 
        CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyConversions
            .AsNoTracking()
            .Where(c => c.FromCurrency == currencyCode)
            .OrderByDescending(c => c.ConversionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CurrencyConversion>> GetFilteredAsync(
        string? fromCurrency = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.CurrencyConversions.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(fromCurrency))
        {
            query = query.Where(c => c.FromCurrency == fromCurrency);
        }

        if (startDate.HasValue)
        {
            query = query.Where(c => c.ConversionDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(c => c.ConversionDate <= endDate.Value);
        }

        return await query
            .OrderByDescending(c => c.ConversionDate)
            .ToListAsync(cancellationToken);
    }
}
