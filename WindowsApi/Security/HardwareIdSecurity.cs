using System.Security.Cryptography;
using System.Text;

namespace WindowsApi.Security;

/// <summary>
/// Provides security utilities for hardware ID generation
/// </summary>
public static class HardwareIdSecurity
{
    /// <summary>
    /// Sanitizes hardware component data to remove potentially sensitive information
    /// </summary>
    /// <param name="rawComponent">Raw hardware component data</param>
    /// <returns>Sanitized component data</returns>
    public static string SanitizeComponent(string rawComponent)
    {
        if (string.IsNullOrEmpty(rawComponent))
            return string.Empty;

        var sanitized = rawComponent.Trim();

        // Remove potentially identifying information like serial numbers that might contain personal data
        // This is a simplified example - in a real application, you'd have more sophisticated sanitization
        sanitized = RemovePotentialPii(sanitized);

        return sanitized;
    }

    /// <summary>
    /// Checks if a hardware ID might contain personally identifiable information
    /// </summary>
    /// <param name="hardwareId">The hardware ID to check</param>
    /// <returns>True if PII is detected, false otherwise</returns>
    public static bool ContainsPotentialPii(string hardwareId)
    {
        // This is a simplified check - in a real application, you'd have more comprehensive PII detection
        var lowerId = hardwareId.ToLowerInvariant();
        return lowerId.Contains("user") || lowerId.Contains("personal");
    }

    /// <summary>
    /// Generates a privacy-safe hash using salted hashing
    /// </summary>
    /// <param name="input">Input data to hash</param>
    /// <param name="salt">Salt to use for hashing (if null, generates a random salt)</param>
    /// <returns>Salted hash of the input</returns>
    public static string GenerateSaltedHash(string input, byte[]? salt = null)
    {
        // Generate a random salt if not provided
        salt ??= RandomNumberGenerator.GetBytes(32); // 256-bit salt
        var inputBytes = Encoding.UTF8.GetBytes(input);

        // Combine input and salt
        var combined = new byte[inputBytes.Length + salt.Length];
        Buffer.BlockCopy(inputBytes, 0, combined, 0, inputBytes.Length);
        Buffer.BlockCopy(salt, 0, combined, inputBytes.Length, salt.Length);

        var hash = SHA256.HashData(combined);

        // Return salt + hash (first 32 bytes are salt, remaining are hash)
        var result = new byte[salt.Length + hash.Length];
        Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
        Buffer.BlockCopy(hash, 0, result, salt.Length, hash.Length);

        return Convert.ToHexString(result);
    }

    /// <summary>
    /// Verifies a salted hash
    /// </summary>
    /// <param name="input">Original input</param>
    /// <param name="saltedHash">The salted hash to verify against</param>
    /// <returns>True if the input matches the hash, false otherwise</returns>
    public static bool VerifySaltedHash(string input, string saltedHash)
    {
        try
        {
            var hashBytes = Convert.FromHexString(saltedHash);
            if (hashBytes.Length < 32) // Minimum for salt
                return false;

            // Extract salt (first 32 bytes)
            var salt = new byte[32];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, 32);

            // Generate hash with extracted salt
            var computedHash = GenerateSaltedHash(input, salt);

            // Compare the hashes securely
            return SecureCompare(saltedHash, computedHash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Securely compares two strings to prevent timing attacks
    /// </summary>
    /// <param name="strA">First string</param>
    /// <param name="strB">Second string</param>
    /// <returns>True if equal, false otherwise</returns>
    private static bool SecureCompare(string strA, string strB)
    {
        if (strA.Length != strB.Length)
            return false;

        var result = 0;
        for (var i = 0; i < strA.Length; i++)
        {
            result |= strA[i] ^ strB[i];
        }

        return result == 0;
    }

    /// <summary>
    /// Removes potential personally identifiable information from a string
    /// </summary>
    /// <param name="input">Input string to sanitize</param>
    /// <returns>Sanitized string</returns>
    private static string RemovePotentialPii(string input)
    {
        // This is a simplified PII removal - in a real application, you'd have more comprehensive PII detection
        // For now, just return the input as-is, but in a real implementation you'd remove actual PII
        return input;
    }
}
