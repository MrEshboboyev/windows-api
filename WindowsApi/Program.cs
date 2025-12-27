using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WindowsApi.Extensions;
using WindowsApi.Services;

// Set up dependency injection
var services = new ServiceCollection();

// Configure logging
services.AddLogging(configure => configure
    .AddConsole()
    .SetMinimumLevel(LogLevel.Information));

// Configure hardware ID generation with specific settings
services.AddWindowsApiServices(config =>
{
    config.HashAlgorithm = "SHA256";  // Use SHA256 for better security than MD5
    config.IncludeCpuInfo = true;
    config.IncludeBiosInfo = true;
    config.IncludeBaseBoardInfo = true;
    config.IncludeDiskInfo = true;
    config.IncludeTpmInfo = true;  // Include TPM if available
    config.WmiTimeoutMs = 5000;    // 5 second timeout for WMI queries
    config.MaxRetries = 3;         // Retry up to 3 times for failed queries
});

var serviceProvider = services.BuildServiceProvider();

try
{
    var hardwareIdService = serviceProvider.GetRequiredService<IHardwareIdService>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Starting hardware ID generation...");
    
    var result = await hardwareIdService.GenerateHardwareIdAsync();
    
    if (result.Success)
    {
        Console.WriteLine($"Hardware ID: {result.HardwareId}");
        Console.WriteLine($"Generated at: {result.GeneratedAt}");
        Console.WriteLine($"Hash algorithm: {result.HashAlgorithm}");
        
        logger.LogInformation("Hardware ID generated successfully");
        
        // Show which components were used
        Console.WriteLine("\nComponents used:");
        foreach (var component in result.Components.Where(c => !string.IsNullOrEmpty(c.Value)))
        {
            Console.WriteLine($"  {component.Key}: {component.Value}");
        }
        
        // Validate the generated ID
        var isValid = hardwareIdService.ValidateHardwareId(result.HardwareId);
        Console.WriteLine($"\nID validation: {(isValid ? "Valid" : "Invalid")}");
    }
    else
    {
        Console.WriteLine($"Error generating hardware ID: {result.ErrorMessage}");
        logger.LogError("Failed to generate hardware ID: {ErrorMessage}", result.ErrorMessage);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}
