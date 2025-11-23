namespace CurrencyExchange.Domain.Entities;

/// <summary>
/// Represents a currency exchange rate entity
/// </summary>
public class CurrencyRate
{
    public Guid Id { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencyName { get; set; } = string.Empty;
    public decimal RateToDKK { get; set; }
    public DateTime FetchedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public CurrencyRate()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateRate(decimal newRate)
    {
        RateToDKK = newRate;
        UpdatedAt = DateTime.UtcNow;
    }
}
