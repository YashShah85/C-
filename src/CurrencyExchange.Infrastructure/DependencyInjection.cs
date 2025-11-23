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
/// Follows Dependency Injection and Open/Closed Principle
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Add DbContext for SQL Server
        services.AddDbContext<CurrencyDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(CurrencyDbContext).Assembly.FullName)));

        // Register Repositories
        services.AddScoped<ICurrencyRateRepository, CurrencyRateRepository>();
        services.AddScoped<ICurrencyConversionRepository, CurrencyConversionRepository>();

        // Register External Services
        services.AddHttpClient<IExternalCurrencyService, NationalbankCurrencyService>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        // Configure Quartz for background jobs
        services.AddQuartz(q =>
        {
            // Create a job key
            var jobKey = new JobKey("CurrencyRateUpdateJob");

            // Register the job with DI container
            q.AddJob<CurrencyRateUpdateJob>(opts => opts.WithIdentity(jobKey));

            // Create a trigger to run every 60 minutes
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("CurrencyRateUpdateJob-trigger")
                .WithCronSchedule("0 0 * * * ?") // Run at the start of every hour
                .WithDescription("Trigger to update currency rates every 60 minutes"));
        });

        // Add Quartz hosted service
        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }
}
