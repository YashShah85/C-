namespace CurrencyExchange.Application.DTOs;

/// <summary>
/// DTO for currency conversion response
/// </summary>
public class ConversionResponseDto
{
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = "DKK";
    public decimal OriginalAmount { get; set; }
    public decimal ConvertedAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public DateTime ConversionDate { get; set; }
}
