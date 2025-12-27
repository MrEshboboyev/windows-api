using System.ComponentModel.DataAnnotations;

namespace WindowsApi.Models;

/// <summary>
/// Configuration model for hardware ID generation
/// </summary>
public class HardwareIdConfiguration
{
    /// <summary>
    /// Hash algorithm to use (SHA256, SHA512, etc.)
    /// </summary>
    [Required]
    public string HashAlgorithm { get; set; } = "SHA256";

    /// <summary>
    /// Whether to include CPU information in the ID
    /// </summary>
    public bool IncludeCpuInfo { get; set; } = true;

    /// <summary>
    /// Whether to include BIOS information in the ID
    /// </summary>
    public bool IncludeBiosInfo { get; set; } = true;

    /// <summary>
    /// Whether to include base board information in the ID
    /// </summary>
    public bool IncludeBaseBoardInfo { get; set; } = true;

    /// <summary>
    /// Whether to include disk information in the ID
    /// </summary>
    public bool IncludeDiskInfo { get; set; } = true;

    /// <summary>
    /// Whether to include TPM information in the ID
    /// </summary>
    public bool IncludeTpmInfo { get; set; } = true;

    /// <summary>
    /// Timeout for WMI queries in milliseconds
    /// </summary>
    public int WmiTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Maximum number of retries for failed WMI queries
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}
