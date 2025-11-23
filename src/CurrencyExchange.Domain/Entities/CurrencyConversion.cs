namespace CurrencyExchange.Domain.Entities;

/// <summary>
/// Represents a currency conversion transaction
/// </summary>
public class CurrencyConversion
{
    public Guid Id { get; set; }
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; }
    public decimal ConvertedAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public DateTime ConversionDate { get; set; }

    public CurrencyConversion()
    {
        Id = Guid.NewGuid();
        ConversionDate = DateTime.UtcNow;
    }

    public CurrencyConversion(string fromCurrency, string toCurrency, decimal originalAmount, 
        decimal convertedAmount, decimal exchangeRate)
    {
        Id = Guid.NewGuid();
        FromCurrency = fromCurrency;
        ToCurrency = toCurrency;
        OriginalAmount = originalAmount;
        ConvertedAmount = convertedAmount;
        ExchangeRate = exchangeRate;
        ConversionDate = DateTime.UtcNow;
    }
}
