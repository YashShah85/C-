using Microsoft.Extensions.DependencyInjection;
using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Application.Services;

namespace CurrencyExchange.Application;

/// <summary>
/// Extension method for registering application services
/// Follows Open/Closed Principle (OCP) for extension
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICurrencyRateService, CurrencyRateService>();
        services.AddScoped<ICurrencyConversionService, CurrencyConversionService>();

        return services;
    }
}
