using CurrencyExchange.Application.DTOs;
using CurrencyExchange.Application.Services;
using CurrencyExchange.Domain.Entities;
using CurrencyExchange.Domain.Exceptions;
using CurrencyExchange.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CurrencyExchange.Tests.Application;

public class CurrencyConversionServiceTests
{
    private readonly Mock<ICurrencyRateRepository> _rateRepositoryMock;
    private readonly Mock<ICurrencyConversionRepository> _conversionRepositoryMock;
    private readonly Mock<ILogger<CurrencyConversionService>> _loggerMock;
    private readonly CurrencyConversionService _service;

    public CurrencyConversionServiceTests()
    {
        _rateRepositoryMock = new Mock<ICurrencyRateRepository>();
        _conversionRepositoryMock = new Mock<ICurrencyConversionRepository>();
        _loggerMock = new Mock<ILogger<CurrencyConversionService>>();
        
        _service = new CurrencyConversionService(
            _rateRepositoryMock.Object,
            _conversionRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ConvertCurrencyAsync_ValidConversion_ReturnsCorrectResult()
    {
        // Arrange
        var request = new ConversionRequestDto
        {
            FromCurrency = "USD",
            Amount = 100m
        };

        var currencyRate = new CurrencyRate
        {
            CurrencyCode = "USD",
            CurrencyName = "US Dollar",
            RateToDKK = 6.85m,
            FetchedAt = DateTime.UtcNow
        };

        _rateRepositoryMock
            .Setup(x => x.GetByCurrencyCodeAsync("USD", It.IsAny<CancellationToken>()))
            .ReturnsAsync(currencyRate);

        _conversionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<CurrencyConversion>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CurrencyConversion c, CancellationToken ct) => c);

        // Act
        var result = await _service.ConvertCurrencyAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.FromCurrency.Should().Be("USD");
        result.ToCurrency.Should().Be("DKK");
        result.OriginalAmount.Should().Be(100m);
        result.ConvertedAmount.Should().Be(685m);
        result.ExchangeRate.Should().Be(6.85m);

        _conversionRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<CurrencyConversion>(), It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task ConvertCurrencyAsync_CurrencyNotFound_ThrowsCurrencyNotFoundException()
    {
        // Arrange
        var request = new ConversionRequestDto
        {
            FromCurrency = "XYZ",
            Amount = 100m
        };

        _rateRepositoryMock
            .Setup(x => x.GetByCurrencyCodeAsync("XYZ", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CurrencyRate?)null);

        // Act & Assert
        await Assert.ThrowsAsync<CurrencyNotFoundException>(() => 
            _service.ConvertCurrencyAsync(request));
    }

    [Fact]
    public async Task ConvertCurrencyAsync_NegativeAmount_ThrowsInvalidConversionException()
    {
        // Arrange
        var request = new ConversionRequestDto
        {
            FromCurrency = "USD",
            Amount = -100m
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidConversionException>(() => 
            _service.ConvertCurrencyAsync(request));
    }

    [Fact]
    public async Task ConvertCurrencyAsync_DKKToDKK_ReturnsOneToOneConversion()
    {
        // Arrange
        var request = new ConversionRequestDto
        {
            FromCurrency = "DKK",
            Amount = 100m
        };

        _conversionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<CurrencyConversion>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CurrencyConversion c, CancellationToken ct) => c);

        // Act
        var result = await _service.ConvertCurrencyAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.FromCurrency.Should().Be("DKK");
        result.ToCurrency.Should().Be("DKK");
        result.OriginalAmount.Should().Be(100m);
        result.ConvertedAmount.Should().Be(100m);
        result.ExchangeRate.Should().Be(1.0m);
    }

    [Fact]
    public async Task GetConversionHistoryAsync_WithFilters_ReturnsFilteredResults()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var conversions = new List<CurrencyConversion>
        {
            new CurrencyConversion("USD", "DKK", 100m, 685m, 6.85m),
            new CurrencyConversion("EUR", "DKK", 100m, 746m, 7.46m)
        };

        _conversionRepositoryMock
            .Setup(x => x.GetFilteredAsync("USD", startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversions.Where(c => c.FromCurrency == "USD"));

        // Act
        var result = await _service.GetConversionHistoryAsync("USD", startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().FromCurrency.Should().Be("USD");
    }
}
