using CurrencyExchange.Domain.Entities;
using CurrencyExchange.Domain.Interfaces;
using CurrencyExchange.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CurrencyExchange.Infrastructure.Repositories;

/// <summary>
/// SQL Server repository implementation for CurrencyRate
/// Follows Repository Pattern and Interface Segregation Principle (ISP)
/// </summary>
public class CurrencyRateRepository : ICurrencyRateRepository
{
    private readonly CurrencyDbContext _context;
    private readonly ILogger<CurrencyRateRepository> _logger;

    public CurrencyRateRepository(
        CurrencyDbContext context,
        ILogger<CurrencyRateRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CurrencyRate?> GetByCurrencyCodeAsync(
        string currencyCode, 
        CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyRates
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.CurrencyCode == currencyCode, cancellationToken);
    }

    public async Task<IEnumerable<CurrencyRate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyRates
            .AsNoTracking()
            .OrderBy(r => r.CurrencyCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<CurrencyRate> AddAsync(
        CurrencyRate currencyRate, 
        CancellationToken cancellationToken = default)
    {
        _context.CurrencyRates.Add(currencyRate);
        await _context.SaveChangesAsync(cancellationToken);
        return currencyRate;
    }

    public async Task UpdateAsync(
        CurrencyRate currencyRate, 
        CancellationToken cancellationToken = default)
    {
        _context.CurrencyRates.Update(currencyRate);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string currencyCode, 
        CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyRates
            .AnyAsync(r => r.CurrencyCode == currencyCode, cancellationToken);
    }

    public async Task<int> BulkUpsertAsync(
        IEnumerable<CurrencyRate> currencyRates, 
        CancellationToken cancellationToken = default)
    {
        var count = 0;

        foreach (var rate in currencyRates)
        {
            var existingRate = await _context.CurrencyRates
                .FirstOrDefaultAsync(r => r.CurrencyCode == rate.CurrencyCode, cancellationToken);

            if (existingRate != null)
            {
                // Update existing rate
                existingRate.UpdateRate(rate.RateToDKK);
                existingRate.CurrencyName = rate.CurrencyName;
                existingRate.FetchedAt = rate.FetchedAt;
                _context.CurrencyRates.Update(existingRate);
            }
            else
            {
                // Add new rate
                _context.CurrencyRates.Add(rate);
            }

            count++;
        }

        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Bulk upserted {Count} currency rates", count);
        
        return count;
    }
}
