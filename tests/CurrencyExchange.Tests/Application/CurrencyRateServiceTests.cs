using CurrencyExchange.Application.Services;
using CurrencyExchange.Domain.Entities;
using CurrencyExchange.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CurrencyExchange.Tests.Application;

public class CurrencyRateServiceTests
{
    private readonly Mock<ICurrencyRateRepository> _rateRepositoryMock;
    private readonly Mock<IExternalCurrencyService> _externalServiceMock;
    private readonly Mock<ILogger<CurrencyRateService>> _loggerMock;
    private readonly CurrencyRateService _service;

    public CurrencyRateServiceTests()
    {
        _rateRepositoryMock = new Mock<ICurrencyRateRepository>();
        _externalServiceMock = new Mock<IExternalCurrencyService>();
        _loggerMock = new Mock<ILogger<CurrencyRateService>>();
        
        _service = new CurrencyRateService(
            _rateRepositoryMock.Object,
            _externalServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllRatesAsync_ReturnsAllRates()
    {
        // Arrange
        var rates = new List<CurrencyRate>
        {
            new CurrencyRate { CurrencyCode = "USD", CurrencyName = "US Dollar", RateToDKK = 6.85m },
            new CurrencyRate { CurrencyCode = "EUR", CurrencyName = "Euro", RateToDKK = 7.46m }
        };

        _rateRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rates);

        // Act
        var result = await _service.GetAllRatesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().CurrencyCode.Should().Be("USD");
    }

    [Fact]
    public async Task GetRateByCurrencyCodeAsync_ExistingCurrency_ReturnsRate()
    {
        // Arrange
        var rate = new CurrencyRate 
        { 
            CurrencyCode = "USD", 
            CurrencyName = "US Dollar", 
            RateToDKK = 6.85m 
        };

        _rateRepositoryMock
            .Setup(x => x.GetByCurrencyCodeAsync("USD", It.IsAny<CancellationToken>()))
            .ReturnsAsync(rate);

        // Act
        var result = await _service.GetRateByCurrencyCodeAsync("USD");

        // Assert
        result.Should().NotBeNull();
        result!.CurrencyCode.Should().Be("USD");
        result.RateToDKK.Should().Be(6.85m);
    }

    [Fact]
    public async Task GetRateByCurrencyCodeAsync_NonExistingCurrency_ReturnsNull()
    {
        // Arrange
        _rateRepositoryMock
            .Setup(x => x.GetByCurrencyCodeAsync("XYZ", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CurrencyRate?)null);

        // Act
        var result = await _service.GetRateByCurrencyCodeAsync("XYZ");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateRatesFromExternalSourceAsync_SuccessfulUpdate_ReturnsCount()
    {
        // Arrange
        var externalRates = new List<CurrencyRate>
        {
            new CurrencyRate { CurrencyCode = "USD", CurrencyName = "US Dollar", RateToDKK = 6.85m },
            new CurrencyRate { CurrencyCode = "EUR", CurrencyName = "Euro", RateToDKK = 7.46m }
        };

        _externalServiceMock
            .Setup(x => x.FetchLatestRatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(externalRates);

        _rateRepositoryMock
            .Setup(x => x.BulkUpsertAsync(It.IsAny<IEnumerable<CurrencyRate>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // Act
        var result = await _service.UpdateRatesFromExternalSourceAsync();

        // Assert
        result.Should().Be(2);
        _externalServiceMock.Verify(
            x => x.FetchLatestRatesAsync(It.IsAny<CancellationToken>()), 
            Times.Once);
        _rateRepositoryMock.Verify(
            x => x.BulkUpsertAsync(It.IsAny<IEnumerable<CurrencyRate>>(), It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task UpdateRatesFromExternalSourceAsync_NoRatesFromExternal_ReturnsZero()
    {
        // Arrange
        _externalServiceMock
            .Setup(x => x.FetchLatestRatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CurrencyRate>());

        // Act
        var result = await _service.UpdateRatesFromExternalSourceAsync();

        // Assert
        result.Should().Be(0);
        _rateRepositoryMock.Verify(
            x => x.BulkUpsertAsync(It.IsAny<IEnumerable<CurrencyRate>>(), It.IsAny<CancellationToken>()), 
            Times.Never);
    }
}
