using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace WindowsApi;

public static class HardwareIdGenerator
{
    public static string GetUniqueHardwareId()
    {
        string cpuId = GetWmiProperty("Win32_Processor", "ProcessorId");
        string biosId = GetWmiProperty("Win32_BIOS", "SerialNumber");
        string baseBoardId = GetWmiProperty("Win32_BaseBoard", "SerialNumber");
        // optional: add disk serial number
        // string diskId = GetWmiProperty("Win32_DiskDrive", "SerialNumber");

        string tpmManufacturerId = GetTpmProperty("ManufacturerID");

        string tpmVersion = GetTpmProperty("ManufacturerVersion"); 

        string rawId = $"{cpuId}-{biosId}-{baseBoardId}-{tpmManufacturerId}-{tpmVersion}";

        return GetHash(rawId);
    }

    private static string GetTpmProperty(string propertyName)
    {
        string result = "";
        try
        {
            ManagementObjectSearcher searcher = new($"SELECT {propertyName} FROM Win32_Tpm");
            foreach (ManagementObject obj in searcher.Get())
            {
                var value = obj[propertyName];
                if (value != null)
                {
                    if (value is byte[] bytes)
                    {
                        result = Convert.ToHexString(bytes);
                    }
                    else
                    {
                        result = value.ToString();
                    }
                    break;
                }
            }
        }
        catch (Exception)
        {
            return "TPM_NOT_AVAILABLE";
        }
        return string.IsNullOrEmpty(result) ? "TPM_NOT_AVAILABLE" : result;
    }

    private static string GetWmiProperty(string className, string propertyName)
    {
        string result = "";
        try
        {
            ManagementObjectSearcher searcher = new($"SELECT {propertyName} FROM {className}");
            foreach (ManagementObject obj in searcher.Get())
            {
                var value = obj[propertyName];
                if (value != null)
                {
                    result = value.ToString();
                    break;
                }
            }
        }
        catch (Exception)
        {
            return "UNKNOWN";
        }
        return result;
    }

    private static string GetHash(string text)
    {
        byte[] inputBytes = Encoding.ASCII.GetBytes(text);
        byte[] hashBytes = MD5.HashData(inputBytes);

        StringBuilder sb = new();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            sb.Append(hashBytes[i].ToString("X2"));
        }
        return sb.ToString();
    }
}
