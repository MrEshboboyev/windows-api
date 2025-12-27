# Windows API Hardware ID Generator

A .NET application that generates unique hardware-based identifiers for Windows systems using WMI (Windows Management Instrumentation) queries to gather system information.

## Tags

.NET, Windows API, Hardware ID, WMI, System Information, Unique Identifier, Security

## Overview

This project provides a robust hardware ID generation service that combines multiple system components to create a unique identifier for a Windows machine. The service collects information from various hardware components including CPU, BIOS, base board, disk drives, and TPM (if available) to generate a unique, persistent identifier.

## Features

- **Multiple Hardware Components**: Gathers information from CPU, BIOS, Base Board, Disk Drives, and TPM
- **Configurable**: Customizable hash algorithms and component selection
- **Robust Error Handling**: Includes retry mechanisms and fallback strategies
- **WMI Query Support**: Uses Windows Management Instrumentation for system information
- **Validation**: Built-in hardware ID validation functionality

## Requirements

- .NET 10.0 or later
- Windows operating system
- Administrative privileges may be required for some WMI queries

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/windows-api.git
   ```

2. Navigate to the project directory:
   ```bash
   cd windows-api
   ```

3. Restore dependencies:
   ```bash
   dotnet restore
   ```

## Configuration

The hardware ID generation can be configured through the `HardwareIdConfiguration` class:

- `HashAlgorithm`: Hash algorithm to use (SHA256, SHA512, SHA1) - defaults to SHA256
- `IncludeCpuInfo`: Include CPU information in the ID - defaults to true
- `IncludeBiosInfo`: Include BIOS information in the ID - defaults to true
- `IncludeBaseBoardInfo`: Include base board information in the ID - defaults to true
- `IncludeDiskInfo`: Include disk information in the ID - defaults to true
- `IncludeTpmInfo`: Include TPM information in the ID - defaults to true
- `WmiTimeoutMs`: Timeout for WMI queries in milliseconds - defaults to 5000ms
- `MaxRetries`: Maximum number of retries for failed WMI queries - defaults to 3

## Usage

The application can be run directly to generate a hardware ID:

```bash
dotnet run
```

This will output the generated hardware ID along with information about which components were used in its creation.

## Dependencies

- `System.Management`: For WMI queries
- `Microsoft.Extensions.DependencyInjection`: For dependency injection
- `Microsoft.Extensions.Logging`: For logging
- `Microsoft.Extensions.Logging.Console`: For console logging
- `System.ComponentModel.Annotations`: For data annotations

## API

The service provides the following methods:

- `GenerateHardwareIdAsync()`: Generates a unique hardware identifier based on multiple system components
- `ValidateHardwareId(string hardwareId)`: Validates if a hardware ID is properly formatted

## Security

The generated hardware ID is created using cryptographic hash functions to ensure uniqueness and prevent reverse engineering of the original hardware information.
