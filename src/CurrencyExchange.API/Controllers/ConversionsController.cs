using CurrencyExchange.Application.DTOs;
using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.API.Controllers;

/// <summary>
/// Controller for currency conversion operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ConversionsController : ControllerBase
{
    private readonly ICurrencyConversionService _conversionService;
    private readonly ILogger<ConversionsController> _logger;

    public ConversionsController(
        ICurrencyConversionService conversionService,
        ILogger<ConversionsController> logger)
    {
        _conversionService = conversionService ?? throw new ArgumentNullException(nameof(conversionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Convert currency amount to Danish Krone (DKK)
    /// </summary>
    /// <param name="request">Conversion request containing currency code and amount</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Conversion result</returns>
    /// <response code="200">Conversion successful</response>
    /// <response code="400">Invalid request</response>
    /// <response code="404">Currency not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("convert")]
    [ProducesResponseType(typeof(ConversionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConvertCurrency(
        [FromBody] ConversionRequestDto request, 
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _conversionService.ConvertCurrencyAsync(request, cancellationToken);
            
            return Ok(result);
        }
        catch (CurrencyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Currency not found: {CurrencyCode}", ex.CurrencyCode);
            return NotFound(new { message = ex.Message, currencyCode = ex.CurrencyCode });
        }
        catch (InvalidConversionException ex)
        {
            _logger.LogWarning(ex, "Invalid conversion request");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing currency conversion");
            return StatusCode(500, new { message = "An error occurred while performing the conversion" });
        }
    }

    /// <summary>
    /// Get conversion history with optional filtering
    /// </summary>
    /// <param name="fromCurrency">Filter by source currency (optional)</param>
    /// <param name="startDate">Filter by start date (optional)</param>
    /// <param name="endDate">Filter by end date (optional)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>List of conversion history</returns>
    /// <response code="200">Returns conversion history</response>
    /// <response code="400">Invalid filter parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("history")]
    [ProducesResponseType(typeof(IEnumerable<ConversionHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetConversionHistory(
        [FromQuery] string? fromCurrency = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                return BadRequest(new { message = "Start date cannot be after end date" });
            }

            var history = await _conversionService.GetConversionHistoryAsync(
                fromCurrency, 
                startDate, 
                endDate, 
                cancellationToken);

            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversion history");
            return StatusCode(500, new { message = "An error occurred while retrieving conversion history" });
        }
    }
}
