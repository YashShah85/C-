using CurrencyExchange.Application.DTOs;
using CurrencyExchange.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.API.Controllers;

/// <summary>
/// Controller for managing currency rates
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CurrencyRatesController : ControllerBase
{
    private readonly ICurrencyRateService _currencyRateService;
    private readonly ILogger<CurrencyRatesController> _logger;

    public CurrencyRatesController(
        ICurrencyRateService currencyRateService,
        ILogger<CurrencyRatesController> logger)
    {
        _currencyRateService = currencyRateService ?? throw new ArgumentNullException(nameof(currencyRateService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all available currency rates
    /// </summary>
    /// <returns>List of all currency rates</returns>
    /// <response code="200">Returns the list of currency rates</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CurrencyRateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllRates(CancellationToken cancellationToken)
    {
        try
        {
            var rates = await _currencyRateService.GetAllRatesAsync(cancellationToken);
            return Ok(rates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving currency rates");
            return StatusCode(500, new { message = "An error occurred while retrieving currency rates" });
        }
    }

    /// <summary>
    /// Get exchange rate for a specific currency
    /// </summary>
    /// <param name="currencyCode">The 3-letter currency code (e.g., USD, EUR)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The currency rate</returns>
    /// <response code="200">Returns the currency rate</response>
    /// <response code="404">Currency not found</response>
    /// <response code="400">Invalid currency code</response>
    [HttpGet("{currencyCode}")]
    [ProducesResponseType(typeof(CurrencyRateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRateByCurrencyCode(
        string currencyCode, 
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Length != 3)
            {
                return BadRequest(new { message = "Currency code must be a 3-letter code (e.g., USD, EUR)" });
            }

            var rate = await _currencyRateService.GetRateByCurrencyCodeAsync(
                currencyCode.ToUpperInvariant(), 
                cancellationToken);

            if (rate == null)
            {
                return NotFound(new { message = $"Currency rate for '{currencyCode}' not found" });
            }

            return Ok(rate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving currency rate for {CurrencyCode}", currencyCode);
            return StatusCode(500, new { message = "An error occurred while retrieving the currency rate" });
        }
    }

    /// <summary>
    /// Manually trigger an update of currency rates from external source
    /// </summary>
    /// <returns>Number of rates updated</returns>
    /// <response code="200">Rates updated successfully</response>
    /// <response code="500">Error updating rates</response>
    [HttpPost("update")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateRates(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Manual currency rate update triggered");
            
            var updatedCount = await _currencyRateService.UpdateRatesFromExternalSourceAsync(cancellationToken);
            
            return Ok(new 
            { 
                message = "Currency rates updated successfully", 
                updatedCount,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating currency rates");
            return StatusCode(500, new { message = "An error occurred while updating currency rates" });
        }
    }
}
