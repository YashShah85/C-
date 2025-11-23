namespace CurrencyExchange.Domain.Exceptions;

/// <summary>
/// Base exception for domain-level errors
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception for currency not found scenarios
/// </summary>
public class CurrencyNotFoundException : DomainException
{
    public string CurrencyCode { get; }

    public CurrencyNotFoundException(string currencyCode) 
        : base($"Currency '{currencyCode}' not found.")
    {
        CurrencyCode = currencyCode;
    }
}

/// <summary>
/// Exception for invalid conversion scenarios
/// </summary>
public class InvalidConversionException : DomainException
{
    public InvalidConversionException(string message) : base(message)
    {
    }
}
