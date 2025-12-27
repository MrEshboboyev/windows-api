using WindowsApi.Models;

namespace WindowsApi.Services;

/// <summary>
/// Interface for hardware ID generation service
/// </summary>
public interface IHardwareIdService
{
    /// <summary>
    /// Generates a unique hardware identifier based on multiple system components
    /// </summary>
    /// <returns>Hardware ID result containing the ID and generation metadata</returns>
    Task<HardwareIdResult> GenerateHardwareIdAsync();
    
    /// <summary>
    /// Validates if a hardware ID is properly formatted
    /// </summary>
    /// <param name="hardwareId">The hardware ID to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    bool ValidateHardwareId(string hardwareId);
}
