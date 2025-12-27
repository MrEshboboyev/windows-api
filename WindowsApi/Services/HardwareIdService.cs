using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using WindowsApi.Models;

namespace WindowsApi.Services;

/// <summary>
/// Implementation of hardware ID generation service with robust error handling and fallback strategies
/// </summary>
public class HardwareIdService(
    IOptions<HardwareIdConfiguration> config,
    ILogger<HardwareIdService> logger
) : IHardwareIdService
{
    private readonly HardwareIdConfiguration _config = config.Value;

    public async Task<HardwareIdResult> GenerateHardwareIdAsync()
    {
        logger.LogInformation("Starting hardware ID generation");
        
        var result = new HardwareIdResult
        {
            GeneratedAt = DateTime.UtcNow,
            Components = [],
            HashAlgorithm = _config.HashAlgorithm
        };

        try
        {
            // Collect hardware components based on configuration
            if (_config.IncludeCpuInfo)
            {
                result.Components["CPU"] = await GetCpuIdAsync() ?? string.Empty;
            }

            if (_config.IncludeBiosInfo)
            {
                result.Components["BIOS"] = await GetBiosIdAsync() ?? string.Empty;
            }

            if (_config.IncludeBaseBoardInfo)
            {
                result.Components["BaseBoard"] = await GetBaseBoardIdAsync() ?? string.Empty;
            }

            if (_config.IncludeDiskInfo)
            {
                result.Components["Disk"] = await GetDiskIdAsync() ?? string.Empty;
            }

            if (_config.IncludeTpmInfo)
            {
                result.Components["TPM"] = await GetTpmIdAsync() ?? string.Empty;
            }

            // Generate the hardware ID from collected components
            var combinedComponents = string.Join(
                "-",
                result.Components.Values.Where(v => !string.IsNullOrEmpty(v)));
            
            if (string.IsNullOrEmpty(combinedComponents))
            {
                logger.LogWarning("No hardware components could be retrieved, using fallback method");
                // Fallback: use machine name and timestamp as a last resort
                combinedComponents = Environment.MachineName + DateTime.UtcNow.Ticks;
            }

            result.HardwareId = ComputeHash(combinedComponents, _config.HashAlgorithm);
            result.Success = true;

            logger.LogInformation("Hardware ID generated successfully: {HardwareId}", 
                result.HardwareId.Substring(0, Math.Min(16, result.HardwareId.Length)) + "...");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while generating hardware ID");
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public bool ValidateHardwareId(string hardwareId)
    {
        if (string.IsNullOrEmpty(hardwareId))
            return false;

        // Validate hardware ID format (should be a hex string of appropriate length)
        // SHA256 produces 64 hex characters, SHA512 produces 128
        var expectedLength = _config.HashAlgorithm.Equals("SHA512", StringComparison.OrdinalIgnoreCase) ? 128 : 64;
        
        if (hardwareId.Length != expectedLength)
            return false;

        // Check if all characters are valid hex characters
        foreach (char c in hardwareId)
        {
            if (!Uri.IsHexDigit(c))
                return false;
        }

        return true;
    }

    private async Task<string?> GetCpuIdAsync()
    {
        return await ExecuteWmiQueryAsync("Win32_Processor", "ProcessorId", "CPU");
    }

    private async Task<string?> GetBiosIdAsync()
    {
        return await ExecuteWmiQueryAsync("Win32_BIOS", "SerialNumber", "BIOS");
    }

    private async Task<string?> GetBaseBoardIdAsync()
    {
        return await ExecuteWmiQueryAsync("Win32_BaseBoard", "SerialNumber", "BaseBoard");
    }

    private async Task<string?> GetDiskIdAsync()
    {
        return await ExecuteWmiQueryAsync("Win32_DiskDrive", "SerialNumber", "Disk");
    }

    private async Task<string?> GetTpmIdAsync()
    {
        try
        {
            // First, check if the Win32_Tpm class exists by querying for its existence
            using var existsSearcher = new ManagementObjectSearcher("SELECT * FROM meta_class WHERE __CLASS = 'Win32_Tpm'");
            var existsCollection = existsSearcher.Get();
            
            if (existsCollection.Count == 0)
            {
                logger.LogDebug("Win32_Tpm class not available on this system");
                return null;
            }
            
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Tpm");
            var collection = searcher.Get();
            
            foreach (ManagementObject obj in collection.Cast<ManagementObject>())
            {
                var manufacturerId = obj["ManufacturerId"];
                var version = obj["ManufacturerVersion"];
                
                if (manufacturerId != null)
                {
                    var id = manufacturerId switch
                    {
                        byte[] bytes => Convert.ToHexString(bytes),
                        _ => manufacturerId.ToString()
                    };
                    
                    obj.Dispose();
                    return id;
                }
                
                obj.Dispose();
            }
        }
        catch (ManagementException ex) when (ex.Message.Contains("Invalid class") || ex.Message.Contains("not found"))
        {
            logger.LogDebug("TPM information could not be retrieved: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "TPM information could not be retrieved");
        }
        
        return null;
    }

    private async Task<string?> ExecuteWmiQueryAsync(string className, string propertyName, string componentName)
    {
        for (int attempt = 0; attempt < _config.MaxRetries; attempt++)
        {
            try
            {
                var options = new ConnectionOptions
                {
                    Timeout = TimeSpan.FromMilliseconds(_config.WmiTimeoutMs)
                };

                var scope = new ManagementScope(@"\\localhost\root\CIMV2", options);
                scope.Connect();

                using var searcher = new ManagementObjectSearcher(scope, 
                    new ObjectQuery($"SELECT {propertyName} FROM {className}"));
                
                var collection = searcher.Get();
                
                foreach (ManagementObject obj in collection)
                {
                    var value = obj[propertyName];
                    if (value != null)
                    {
                        var result = value.ToString();
                        obj.Dispose();
                        return result;
                    }
                    obj.Dispose();
                }
                
                break; // Success - exit retry loop
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, 
                    "Attempt {Attempt} failed to retrieve {ComponentName} information. Error: {Error}", 
                    attempt + 1, componentName, ex.Message);
                
                if (attempt < _config.MaxRetries - 1)
                {
                    // Wait before retrying (exponential backoff)
                    await Task.Delay((int)Math.Pow(2, attempt) * 100);
                }
            }
        }
        
        logger.LogWarning("{ComponentName} information could not be retrieved after {MaxRetries} attempts", 
            componentName, _config.MaxRetries);
        
        return null;
    }

    private static string ComputeHash(string input, string algorithm)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);

        return algorithm.ToUpperInvariant() switch
        {
            "SHA256" => ComputeSha256Hash(inputBytes),
            "SHA512" => ComputeSha512Hash(inputBytes),
            "SHA1" => ComputeSha1Hash(inputBytes),
            _ => ComputeSha256Hash(inputBytes) // Default to SHA256
        };
    }

    private static string ComputeSha256Hash(byte[] input)
    {
        byte[] hashBytes = SHA256.HashData(input);
        return Convert.ToHexString(hashBytes).ToUpperInvariant();
    }

    private static string ComputeSha512Hash(byte[] input)
    {
        byte[] hashBytes = SHA512.HashData(input);
        return Convert.ToHexString(hashBytes).ToUpperInvariant();
    }

    private static string ComputeSha1Hash(byte[] input)
    {
        byte[] hashBytes = SHA1.HashData(input);
        return Convert.ToHexString(hashBytes).ToUpperInvariant();
    }
}
