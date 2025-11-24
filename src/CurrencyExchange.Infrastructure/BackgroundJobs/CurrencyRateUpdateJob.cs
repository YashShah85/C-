using CurrencyExchange.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CurrencyExchange.Infrastructure.BackgroundJobs;

/// <summary>
/// Scheduled job to update currency rates every 60 minutes
/// </summary>
[DisallowConcurrentExecution]
public class CurrencyRateUpdateJob : IJob
{
    private readonly ICurrencyRateService _currencyRateService;
    private readonly ILogger<CurrencyRateUpdateJob> _logger;

    public CurrencyRateUpdateJob(
        ICurrencyRateService currencyRateService,
        ILogger<CurrencyRateUpdateJob> logger)
    {
        _currencyRateService = currencyRateService ?? throw new ArgumentNullException(nameof(currencyRateService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting scheduled currency rate update job");

        try
        {
            var updatedCount = await _currencyRateService.UpdateRatesFromExternalSourceAsync(
                context.CancellationToken);

            _logger.LogInformation(
                "Currency rate update job completed successfully. Updated {Count} rates", 
                updatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing currency rate update job");
            
            throw new JobExecutionException(ex, refireImmediately: false);
        }
    }
}
