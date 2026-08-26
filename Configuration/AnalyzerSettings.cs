#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Constants;

namespace SqlQueryAnalyzer.Configuration;

/// <summary>
/// Centralized configuration for the SQL query analyzer.
/// Loads settings from JSON files or environment variables.
/// Validates configuration and provides sensible defaults.
/// </summary>
public class AnalyzerSettings
{
    public DatabaseSettings Database { get; set; } = new();
    public AnalysisSettings Analysis { get; set; } = new();
    public CacheSettings Cache { get; set; } = new();
    public PerformanceSettings Performance { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();

    /// <summary>
    /// Loads configuration from JSON file.
    /// Merges with environment variables (ENV overrides JSON).
    /// </summary>
    public static AnalyzerSettings LoadFromFile(string filePath, ILogger? logger = null)
    {
        var settings = new AnalyzerSettings();

        try
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var loaded = JsonSerializer.Deserialize<AnalyzerSettings>(json, options);

                if (loaded != null)
                {
                    settings = loaded;
                    logger?.LogInformation($"Configuration loaded from {filePath}");
                }
            }
            else
            {
                logger?.LogWarning($"Configuration file not found: {filePath}. Using defaults.");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, $"Error loading configuration from {filePath}");
        }

        // Override with environment variables
        settings.ApplyEnvironmentVariables();

        return settings;
    }

    /// <summary>
    /// Saves current configuration to JSON file.
    /// </summary>
    public void SaveToFile(string filePath)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save configuration to {filePath}", ex);
        }
    }

    /// <summary>
    /// Applies environment variable overrides to settings.
    /// Follows pattern: SQA_SECTION_PROPERTY
    /// </summary>
    private void ApplyEnvironmentVariables()
    {
        var maxThreads = Environment.GetEnvironmentVariable("SQA_ANALYSIS_MAX_THREADS");
        if (int.TryParse(maxThreads, out var threads))
        {
            Analysis.MaxThreads = threads;
        }

        var cacheEnabled = Environment.GetEnvironmentVariable("SQA_CACHE_ENABLED");
        if (bool.TryParse(cacheEnabled, out var enabled))
        {
            Cache.Enabled = enabled;
        }

        var dbConnection = Environment.GetEnvironmentVariable("SQA_DATABASE_CONNECTION_STRING");
        if (!string.IsNullOrEmpty(dbConnection))
        {
            Database.ConnectionString = dbConnection;
        }
    }

    /// <summary>
    /// Validates configuration settings.
    /// Returns list of validation errors.
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (Analysis.MaxThreads < 1)
            errors.Add("Analysis.MaxThreads must be at least 1");

        if (Cache.MaxSizeBytes < 1024)
            errors.Add("Cache.MaxSizeBytes must be at least 1 KB");

        if (Performance.TimeoutSeconds < 1)
            errors.Add("Performance.TimeoutSeconds must be at least 1");

        return errors;
    }
     public override string ToString() => $"AnalyzerSettings {{ Database = {Database}, Analysis = {Analysis}, Cache = {Cache}, Performance = {Performance}, Logging = {Logging}, Provider = {Database.Provider} }}";
 => "AnalyzerSettings { Database = {Database}, Analysis = {Analysis}, Cache = {Cache}, Performance = {Performance}, Logging = {Logging}, Provider = {Database.Provider} }";
}

/// <summary>
/// Database connection settings.
/// </summary>
public class DatabaseSettings
{
    public string Provider { get; set; } = "SqlServer"; // SqlServer, PostgreSQL, MySQL
    public string ConnectionString { get; set; } = "Server=localhost;Database=SqlAnalyzer;";
    public int ConnectionPoolSize { get; set; } = 10;
    public int ConnectionTimeoutSeconds { get; set; } = 5;
    public bool EnableConnectionLogging { get; set; } = false;
}

/// <summary>
/// Analysis-specific settings.
/// </summary>
public class AnalysisSettings
{
    public int MaxThreads { get; set; } = Environment.ProcessorCount;
    public bool DetectNPlusOne { get; set; } = true;
    public bool DetectMissingIndexes { get; set; } = true;
    public bool DetectJoinIssues { get; set; } = true;
    public bool AnalyzeExecutionPlans { get; set; } = true;
    public double CriticalIssueSensitivity { get; set; } = 0.8; // 0-1 scale
    public bool EnableDetailedLogging { get; set; } = false;

    /// <summary>
    /// Configurable thresholds that map estimated row count or cost to
    /// missing-index warning severity (Info / Warning / Critical).
    /// </summary>
    public IndexSeverityThresholds IndexSeverity { get; set; } = new();

    /// <summary>
    /// Query patterns (regex strings) whose matches are excluded from all
    /// detectors. Useful for suppressing known false positives from ORMs
    /// such as Entity Framework or Dapper.
    /// </summary>
    public List<string> IgnorePatterns { get; set; } = [];
}

/// <summary>
/// Caching settings.
/// </summary>
public class CacheSettings
{
    public bool Enabled { get; set; } = true;
    public string Provider { get; set; } = "InMemory"; // InMemory, Redis
    public int MaxEntries { get; set; } = 10000;
    public int MaxSizeBytes { get; set; } = 1024 * 1024 * 100; // 100 MB
    public int ExpirationSeconds { get; set; } = 3600; // 1 hour
    public string? RedisConnectionString { get; set; }
}

/// <summary>
/// Performance and resource settings.
/// </summary>
public class PerformanceSettings
{
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxQueryLength { get; set; } = 1024 * 1024; // 1 MB
    public int RateLimitQueriesPerSecond { get; set; } = 100;
    public int MaxConcurrentAnalysis { get; set; } = 10;
    public bool EnableBatching { get; set; } = true;
    public int BatchSize { get; set; } = 50;
}

/// <summary>
/// Logging settings.
/// </summary>
public class LoggingSettings
{
    public string MinimumLevel { get; set; } = "Information"; // Debug, Information, Warning, Error
    public bool ConsoleLogging { get; set; } = true;
    public bool FileLogging { get; set; } = false;
    public string? LogFilePath { get; set; }
    public int LogMaxFileSizeBytes { get; set; } = 1024 * 1024 * 10; // 10 MB
    public int LogMaxBackupFiles { get; set; } = 5;
}

/// <summary>
/// Configurable thresholds that map estimated row count or query cost to
/// missing-index warning severity levels (Info / Warning / Critical).
///
/// Configure via analyzer.json:
/// <code>
/// {
///   "Analysis": {
///     "IndexSeverity": {
///       "InfoMaxRows": 10000,
///       "WarningMaxRows": 1000000,
///       "InfoMaxCost": 10.0,
///       "WarningMaxCost": 100.0
///     }
///   }
/// }
/// </code>
/// </summary>
public class IndexSeverityThresholds
{
    /// <summary>Row count at or below which a missing-index issue is Info severity.</summary>
    public long InfoMaxRows { get; set; } = 10_000;

    /// <summary>Row count at or below which a missing-index issue is Warning severity.
    /// Anything above this threshold is Critical.</summary>
    public long WarningMaxRows { get; set; } = 1_000_000;

    /// <summary>Estimated cost at or below which a missing-index issue is Info severity.</summary>
    public double InfoMaxCost { get; set; } = 10.0;

    /// <summary>Estimated cost at or below which a missing-index issue is Warning severity.
    /// Anything above this threshold is Critical.</summary>
    public double WarningMaxCost { get; set; } = 100.0;

    /// <summary>
    /// Resolves the appropriate severity for a missing-index warning.
    /// Row count takes precedence when both arguments are supplied.
    /// Falls back to Warning when neither is provided.
    /// </summary>
    public IssueSeverity ResolveSeverity(long? rowCount = null, double? estimatedCost = null)
    {
        if (rowCount.HasValue)
        {
            if (rowCount.Value <= InfoMaxRows)   return IssueSeverity.Info;
            if (rowCount.Value <= WarningMaxRows) return IssueSeverity.Warning;
            return IssueSeverity.Critical;
        }

        if (estimatedCost.HasValue)
        {
            if (estimatedCost.Value <= InfoMaxCost)   return IssueSeverity.Info;
            if (estimatedCost.Value <= WarningMaxCost) return IssueSeverity.Warning;
            return IssueSeverity.Critical;
        }

        return IssueSeverity.Warning;
    }
}

/// <summary>
/// Factory for creating settings instances.
/// Encapsulates configuration loading logic.
/// </summary>
public static class AnalyzerSettingsFactory
{
    /// <summary>
    /// Creates settings with sensible defaults.
    /// </summary>
    public static AnalyzerSettings CreateDefault()
    {
        return new AnalyzerSettings();
    }

    /// <summary>
    /// Creates settings by loading from standard locations.
    /// Tries: local config file, app directory, home directory.
    /// </summary>
    public static AnalyzerSettings CreateFromStandardLocations(ILogger? logger = null)
    {
        var locations = new[]
        {
            "appsettings.json",
            Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".sqlanalyzer", "config.json")
        };

        foreach (var location in locations)
        {
            if (File.Exists(location))
            {
                return AnalyzerSettings.LoadFromFile(location, logger);
            }
        }

        logger?.LogWarning("No configuration file found. Using defaults.");
        return CreateDefault();
    }
}
