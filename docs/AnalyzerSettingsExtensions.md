# AnalyzerSettingsExtensions

The `AnalyzerSettingsExtensions` class provides extension methods for `AnalyzerSettings` to simplify common configuration tasks. These methods encapsulate frequently used logic for retrieving active features, validating connection strings, and generating configuration summaries.

## API

### Methods

#### `GetActiveDetectionFeatures`
*   **Signature:** `public static IReadOnlyList<string> GetActiveDetectionFeatures(this AnalyzerSettings settings)`
*   **Description:** Gets a list of enabled analysis features.
*   **Parameters:**
    *   `settings` (`AnalyzerSettings`): The settings instance.
*   **Returns:** A read-only list of enabled feature names.
*   **Exceptions:**
    *   Throws `ArgumentNullException` when `<paramref name="settings"/>` is null.
*   **Remarks:** This method checks the boolean flags in the `Analysis` settings and returns a list of strings representing the names of enabled detection features.

#### `IsConnectionValid`
*   **Signature:** `public static bool IsConnectionValid(this AnalyzerSettings settings)`
*   **Description:** Validates if the database connection string is properly configured.
*   **Parameters:**
    *   `settings` (`AnalyzerSettings`): The settings instance.
*   **Returns:** True if the connection string is not null or whitespace; otherwise false.
*   **Exceptions:**
    *   Throws `ArgumentNullException` when `<paramref name="settings"/>` is null.
    *   Throws `ArgumentNullException` when `<paramref name="settings.Database"/>` is null.
*   **Remarks:** This method provides a quick way to check if the essential database connection configuration is present before attempting to establish a connection.

#### `GetSummary`
*   **Signature:** `public static string GetSummary(this AnalyzerSettings settings)`
*   **Description:** Provides a comprehensive summary string of the key settings in the current configuration. Includes database provider, analysis settings, and cache configuration.
*   **Parameters:**
    *   `settings` (`AnalyzerSettings`): The settings instance.
*   **Returns:** A formatted summary string with invariant culture formatting.
*   **Exceptions:**
    *   Throws `ArgumentNullException` when `<paramref name="settings"/>` is null.
*   **Remarks:** This method creates a human-readable summary of the current configuration state, useful for logging, debugging, or displaying configuration information to users. The format uses invariant culture to ensure consistent output regardless of the system's locale settings.

## Usage

### Example 1: Getting Active Detection Features
This example demonstrates how to retrieve the list of currently enabled analysis features from an analyzer settings instance.

```csharp
using System;
using SqlQueryAnalyzer.Configuration;

public class FeatureReporter
{
    public void ReportActiveFeatures(AnalyzerSettings settings)
    {
        var activeFeatures = settings.GetActiveDetectionFeatures();
        
        Console.WriteLine("Active detection features:");
        foreach (var feature in activeFeatures)
        {
            Console.WriteLine($"- {feature}");
        }
        
        // Output might be:
        // Active detection features:
        // - DetectNPlusOne
        // - DetectMissingIndexes
        // - AnalyzeExecutionPlans
    }
}
```

### Example 2: Validating Database Connection
This example shows how to validate that the database connection string is properly configured before attempting to use the analyzer.

```csharp
using System;
using SqlQueryAnalyzer.Configuration;

public class ConnectionValidator
{
    public bool ValidateConnection(AnalyzerSettings settings)
    {
        if (!settings.IsConnectionValid())
        {
            Console.WriteLine("Database connection is not properly configured.");
            Console.WriteLine("Please check that the connection string is set and not empty.");
            return false;
        }
        
        Console.WriteLine("Database connection configuration is valid.");
        return true;
    }
}
```

### Example 3: Generating Configuration Summary
This example demonstrates how to generate a summary string of the current analyzer settings for logging or display purposes.

```csharp
using System;
using SqlQueryAnalyzer.Configuration;

public class ConfigurationLogger
{
    public void LogConfiguration(AnalyzerSettings settings)
    {
        string summary = settings.GetSummary();
        Console.WriteLine($"Current analyzer configuration: {summary}");
        
        // Output might be:
        // Current analyzer configuration: Database: System.Data.SqlClient, ConnectionPool: 20, MaxThreads: 4, Cache: Enabled (Memory), Analysis: NPlusOne=True, MissingIndexes=True, JoinIssues=False, ExecutionPlans=True
    }
}
```