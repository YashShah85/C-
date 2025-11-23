using CurrencyExchange.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Infrastructure.Data;

/// <summary>
/// SQL Server DbContext for currency exchange data
/// </summary>
public class CurrencyDbContext : DbContext
{
    public CurrencyDbContext(DbContextOptions<CurrencyDbContext> options) : base(options)
    {
    }

    public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();
    public DbSet<CurrencyConversion> CurrencyConversions => Set<CurrencyConversion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure CurrencyRate entity
        modelBuilder.Entity<CurrencyRate>(entity =>
        {
            entity.ToTable("CurrencyRates");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.CurrencyCode)
                .IsRequired()
                .HasMaxLength(3);
            
            entity.Property(e => e.CurrencyName)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.RateToDKK)
                .HasPrecision(18, 6);
            
            entity.Property(e => e.FetchedAt)
                .IsRequired();
            
            entity.Property(e => e.CreatedAt)
                .IsRequired();

            // Create unique index on CurrencyCode for efficient lookups
            entity.HasIndex(e => e.CurrencyCode)
                .IsUnique()
                .HasDatabaseName("IX_CurrencyRates_CurrencyCode");

            // Create index on FetchedAt for time-based queries
            entity.HasIndex(e => e.FetchedAt)
                .HasDatabaseName("IX_CurrencyRates_FetchedAt");
        });

        // Configure CurrencyConversion entity
        modelBuilder.Entity<CurrencyConversion>(entity =>
        {
            entity.ToTable("CurrencyConversions");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.FromCurrency)
                .IsRequired()
                .HasMaxLength(3);
            
            entity.Property(e => e.ToCurrency)
                .IsRequired()
                .HasMaxLength(3);
            
            entity.Property(e => e.OriginalAmount)
                .HasPrecision(18, 2);
            
            entity.Property(e => e.ConvertedAmount)
                .HasPrecision(18, 2);
            
            entity.Property(e => e.ExchangeRate)
                .HasPrecision(18, 6);
            
            entity.Property(e => e.ConversionDate)
                .IsRequired();

            // Create index on FromCurrency for filtering
            entity.HasIndex(e => e.FromCurrency)
                .HasDatabaseName("IX_CurrencyConversions_FromCurrency");

            // Create index on ConversionDate for date range queries
            entity.HasIndex(e => e.ConversionDate)
                .HasDatabaseName("IX_CurrencyConversions_ConversionDate");

            // Create composite index for common query patterns
            entity.HasIndex(e => new { e.FromCurrency, e.ConversionDate })
                .HasDatabaseName("IX_CurrencyConversions_FromCurrency_ConversionDate");
        });
    }
}
