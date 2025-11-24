using System.Xml.Linq;
using CurrencyExchange.Domain.Entities;
using CurrencyExchange.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CurrencyExchange.Infrastructure.ExternalServices;

/// <summary>
/// Service to fetch currency rates from Nationalbanken API
/// </summary>
public class NationalbankCurrencyService : IExternalCurrencyService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NationalbankCurrencyService> _logger;
    private const string ApiUrl = "https://www.nationalbanken.dk/api/currencyratesxml";

    public NationalbankCurrencyService(
        HttpClient httpClient,
        ILogger<NationalbankCurrencyService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<CurrencyRate>> FetchLatestRatesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching currency rates from Nationalbanken API");

            var response = await _httpClient.GetAsync(ApiUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            
            var rates = ParseXmlResponse(content);
            
            _logger.LogInformation("Successfully fetched {Count} currency rates", rates.Count());
            
            return rates;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching currency rates from Nationalbanken");
            throw new Exception("Failed to fetch currency rates from external API", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing currency rates from Nationalbanken");
            throw;
        }
    }

    private IEnumerable<CurrencyRate> ParseXmlResponse(string xmlContent)
    {
        var rates = new List<CurrencyRate>();

        try
        {
            var document = XDocument.Parse(xmlContent);
            var ns = document.Root?.GetDefaultNamespace() ?? XNamespace.None;
            
            var dailyRates = document.Descendants(ns + "dailyrates").FirstOrDefault();
            if (dailyRates == null)
            {
                _logger.LogWarning("No dailyrates element found in XML response");
                return rates;
            }

            var fetchDate = dailyRates.Attribute("id")?.Value;
            var fetchedAt = DateTime.TryParse(fetchDate, out var parsedDate) 
                ? parsedDate 
                : DateTime.UtcNow;

            var currencies = document.Descendants(ns + "currency");

            foreach (var currency in currencies)
            {
                try
                {
                    var code = currency.Attribute("code")?.Value;
                    var desc = currency.Attribute("desc")?.Value;
                    var rateElement = currency.Attribute("rate")?.Value;

                    if (string.IsNullOrWhiteSpace(code) || rateElement == null)
                        continue;

                    if (!decimal.TryParse(rateElement, 
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, 
                        out var rate))
                    {
                        _logger.LogWarning("Failed to parse rate for currency {Code}", code);
                        continue;
                    }

                    var currencyRate = new CurrencyRate
                    {
                        CurrencyCode = code,
                        CurrencyName = desc ?? code,
                        RateToDKK = rate / 100,
                        FetchedAt = fetchedAt
                    };

                    rates.Add(currencyRate);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing individual currency element");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing XML response");
            throw;
        }

        return rates;
    }
}
