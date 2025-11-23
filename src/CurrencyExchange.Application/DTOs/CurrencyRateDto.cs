namespace CurrencyExchange.Application.DTOs;

/// <summary>
/// DTO for currency rate response
/// </summary>
public class CurrencyRateDto
{
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencyName { get; set; } = string.Empty;
    public decimal RateToDKK { get; set; }
    public DateTime FetchedAt { get; set; }
}
