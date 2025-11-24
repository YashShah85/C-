using CurrencyExchange.Domain.Interfaces;
using CurrencyExchange.Infrastructure.BackgroundJobs;
using CurrencyExchange.Infrastructure.Data;
using CurrencyExchange.Infrastructure.ExternalServices;
using CurrencyExchange.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace CurrencyExchange.Infrastructure;

/// <summary>
/// Extension method for registering infrastructure services
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDbContext<CurrencyDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(CurrencyDbContext).Assembly.FullName)));

        services.AddScoped<ICurrencyRateRepository, CurrencyRateRepository>();
        services.AddScoped<ICurrencyConversionRepository, CurrencyConversionRepository>();

        services.AddHttpClient<IExternalCurrencyService, NationalbankCurrencyService>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        services.AddQuartz(q =>
        {
            var jobKey = new JobKey("CurrencyRateUpdateJob");

            q.AddJob<CurrencyRateUpdateJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("CurrencyRateUpdateJob-trigger")
                .WithCronSchedule("0 0 * * * ?") 
                .WithDescription("Trigger to update currency rates every 60 minutes"));
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }
}
