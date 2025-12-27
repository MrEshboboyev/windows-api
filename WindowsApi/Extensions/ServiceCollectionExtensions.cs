using Microsoft.Extensions.DependencyInjection;
using WindowsApi.Models;
using WindowsApi.Services;

namespace WindowsApi.Extensions;

/// <summary>
/// Extension methods for configuring Windows API services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Windows API services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The hardware ID configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddWindowsApiServices(
        this IServiceCollection services, 
        HardwareIdConfiguration? configuration = null)
    {
        // Use provided configuration or create default
        var config = configuration ?? new HardwareIdConfiguration();
        
        services.AddSingleton(config);
        services.AddScoped<IHardwareIdService, HardwareIdService>();
        
        return services;
    }

    /// <summary>
    /// Adds Windows API services with configuration from IOptions
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Action to configure the hardware ID settings</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddWindowsApiServices(
        this IServiceCollection services, 
        Action<HardwareIdConfiguration> configure)
    {
        var config = new HardwareIdConfiguration();
        configure(config);
        
        services.AddSingleton(config);
        services.AddScoped<IHardwareIdService, HardwareIdService>();
        
        return services;
    }
}
