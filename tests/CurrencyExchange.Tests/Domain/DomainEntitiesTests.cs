using CurrencyExchange.Domain.Entities;
using FluentAssertions;

namespace CurrencyExchange.Tests.Domain;

public class CurrencyRateTests
{
    [Fact]
    public void CurrencyRate_Creation_SetsDefaultValues()
    {
        // Act
        var rate = new CurrencyRate();

        // Assert
        rate.Id.Should().NotBeEmpty();
        rate.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        rate.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void UpdateRate_UpdatesRateAndTimestamp()
    {
        // Arrange
        var rate = new CurrencyRate
        {
            CurrencyCode = "USD",
            RateToDKK = 6.50m
        };

        // Act
        rate.UpdateRate(6.85m);

        // Assert
        rate.RateToDKK.Should().Be(6.85m);
        rate.UpdatedAt.Should().NotBeNull();
        rate.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}

public class CurrencyConversionTests
{
    [Fact]
    public void CurrencyConversion_Creation_SetsDefaultValues()
    {
        // Act
        var conversion = new CurrencyConversion();

        // Assert
        conversion.Id.Should().NotBeEmpty();
        conversion.ConversionDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CurrencyConversion_ParameterizedConstructor_SetsValues()
    {
        // Act
        var conversion = new CurrencyConversion("USD", "DKK", 100m, 685m, 6.85m);

        // Assert
        conversion.FromCurrency.Should().Be("USD");
        conversion.ToCurrency.Should().Be("DKK");
        conversion.OriginalAmount.Should().Be(100m);
        conversion.ConvertedAmount.Should().Be(685m);
        conversion.ExchangeRate.Should().Be(6.85m);
        conversion.Id.Should().NotBeEmpty();
        conversion.ConversionDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
