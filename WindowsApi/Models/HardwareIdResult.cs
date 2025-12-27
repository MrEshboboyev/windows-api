using System.ComponentModel.DataAnnotations;

namespace WindowsApi.Models;

/// <summary>
/// Result model for hardware ID generation operations
/// </summary>
public class HardwareIdResult
{
    /// <summary>
    /// The generated hardware ID
    /// </summary>
    [Required]
    public string HardwareId { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the ID was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Collection of components used to generate the ID
    /// </summary>
    public Dictionary<string, string> Components { get; set; } = [];

    /// <summary>
    /// Indicates if the generation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Any error messages if generation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Hash algorithm used for generation
    /// </summary>
    public string HashAlgorithm { get; set; } = "SHA256";
}
