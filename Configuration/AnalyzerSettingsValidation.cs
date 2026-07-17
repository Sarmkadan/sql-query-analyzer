#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for AnalyzerSettings configuration
// =====================================================================

namespace SqlQueryAnalyzer.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="AnalyzerSettings"/> configuration.
/// </summary>
public static class AnalyzerSettingsValidation
{
    /// <summary>
    /// Validates the provided <see cref="AnalyzerSettings"/> instance.
    /// Returns a list of human-readable validation errors.
    /// </summary>
    /// <param name="value">The settings to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this AnalyzerSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Database settings
        errors.AddRange(value.Database?.Validate() ?? ["Database settings cannot be null."]);

        // Validate Analysis settings
        errors.AddRange(value.Analysis?.Validate() ?? ["Analysis settings cannot be null."]);

        // Validate Cache settings
        errors.AddRange(value.Cache?.Validate() ?? ["Cache settings cannot be null."]);

        // Validate Performance settings
        errors.AddRange(value.Performance?.Validate() ?? ["Performance settings cannot be null."]);

        // Validate Logging settings
        errors.AddRange(value.Logging?.Validate() ?? ["Logging settings cannot be null."]);

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="DatabaseSettings"/> instance.
    /// </summary>
    /// <param name="value">The database settings to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DatabaseSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Provider))
        {
            errors.Add("Database.Provider cannot be null or whitespace.");
        }
        else if (!IsValidProvider(value.Provider))
        {
            errors.Add("Database.Provider must be one of: SqlServer, PostgreSQL, MySQL.");
        }

        if (string.IsNullOrWhiteSpace(value.ConnectionString))
        {
            errors.Add("Database.ConnectionString cannot be null or whitespace.");
        }

        if (value.ConnectionPoolSize < 1)
        {
            errors.Add("Database.ConnectionPoolSize must be at least 1.");
        }

        if (value.ConnectionTimeoutSeconds < 1)
        {
            errors.Add("Database.ConnectionTimeoutSeconds must be at least 1.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="AnalysisSettings"/> instance.
    /// </summary>
    /// <param name="value">The analysis settings to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this AnalysisSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (value.MaxThreads < 1)
        {
            errors.Add("Analysis.MaxThreads must be at least 1.");
        }

        if (value.CriticalIssueSensitivity is < 0 or > 1)
        {
            errors.Add("Analysis.CriticalIssueSensitivity must be between 0 and 1 (inclusive).");
        }

        errors.AddRange(value.IndexSeverity.Validate());

        if (value.IgnorePatterns is null)
        {
            errors.Add("Analysis.IgnorePatterns cannot be null.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="CacheSettings"/> instance.
    /// </summary>
    /// <param name="value">The cache settings to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CacheSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Provider))
        {
            errors.Add("Cache.Provider cannot be null or whitespace.");
        }
        else if (!IsValidCacheProvider(value.Provider))
        {
            errors.Add("Cache.Provider must be one of: InMemory, Redis.");
        }

        if (value.MaxEntries < 0)
        {
            errors.Add("Cache.MaxEntries must be non-negative.");
        }

        if (value.MaxSizeBytes < 1024)
        {
            errors.Add("Cache.MaxSizeBytes must be at least 1 KB (1024 bytes).");
        }

        if (value.ExpirationSeconds < 0)
        {
            errors.Add("Cache.ExpirationSeconds must be non-negative.");
        }

        if (value.Provider.Equals("Redis", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(value.RedisConnectionString))
        {
            errors.Add("Cache.RedisConnectionString is required when Cache.Provider is Redis.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="PerformanceSettings"/> instance.
    /// </summary>
    /// <param name="value">The performance settings to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PerformanceSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (value.TimeoutSeconds < 1)
        {
            errors.Add("Performance.TimeoutSeconds must be at least 1.");
        }

        if (value.MaxQueryLength < 1)
        {
            errors.Add("Performance.MaxQueryLength must be at least 1.");
        }

        if (value.RateLimitQueriesPerSecond < 1)
        {
            errors.Add("Performance.RateLimitQueriesPerSecond must be at least 1.");
        }

        if (value.MaxConcurrentAnalysis < 1)
        {
            errors.Add("Performance.MaxConcurrentAnalysis must be at least 1.");
        }

        if (value.BatchSize < 1)
        {
            errors.Add("Performance.BatchSize must be at least 1.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="LoggingSettings"/> instance.
    /// </summary>
    /// <param name="value">The logging settings to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this LoggingSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(value.MinimumLevel))
        {
            errors.Add("Logging.MinimumLevel cannot be null or whitespace.");
        }
        else if (!IsValidLogLevel(value.MinimumLevel))
        {
            errors.Add("Logging.MinimumLevel must be one of: Debug, Information, Warning, Error.");
        }

        if (value.LogMaxFileSizeBytes < 1024)
        {
            errors.Add("Logging.LogMaxFileSizeBytes must be at least 1 KB (1024 bytes).");
        }

        if (value.LogMaxBackupFiles < 0)
        {
            errors.Add("Logging.LogMaxBackupFiles must be non-negative.");
        }

        if (value.FileLogging && string.IsNullOrWhiteSpace(value.LogFilePath))
        {
            errors.Add("Logging.LogFilePath is required when Logging.FileLogging is true.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="IndexSeverityThresholds"/> instance.
    /// </summary>
    /// <param name="value">The index severity thresholds to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this IndexSeverityThresholds value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (value.InfoMaxRows < 0)
        {
            errors.Add("Analysis.IndexSeverity.InfoMaxRows must be non-negative.");
        }

        if (value.WarningMaxRows < 0)
        {
            errors.Add("Analysis.IndexSeverity.WarningMaxRows must be non-negative.");
        }

        if (value.InfoMaxRows > value.WarningMaxRows)
        {
            errors.Add("Analysis.IndexSeverity.InfoMaxRows must be less than or equal to Analysis.IndexSeverity.WarningMaxRows.");
        }

        if (value.InfoMaxCost < 0)
        {
            errors.Add("Analysis.IndexSeverity.InfoMaxCost must be non-negative.");
        }

        if (value.WarningMaxCost < 0)
        {
            errors.Add("Analysis.IndexSeverity.WarningMaxCost must be non-negative.");
        }

        if (value.InfoMaxCost > value.WarningMaxCost)
        {
            errors.Add("Analysis.IndexSeverity.InfoMaxCost must be less than or equal to Analysis.IndexSeverity.WarningMaxCost.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the provider name is valid.
    /// </summary>
    /// <param name="provider">The provider name to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    private static bool IsValidProvider(string provider)
    {
        return provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase) ||
               provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase) ||
               provider.Equals("MySQL", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the cache provider name is valid.
    /// </summary>
    /// <param name="provider">The cache provider name to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    private static bool IsValidCacheProvider(string provider)
    {
        return provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase) ||
               provider.Equals("Redis", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the log level is valid.
    /// </summary>
    /// <param name="level">The log level to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    private static bool IsValidLogLevel(string level)
    {
        return level.Equals("Debug", StringComparison.OrdinalIgnoreCase) ||
               level.Equals("Information", StringComparison.OrdinalIgnoreCase) ||
               level.Equals("Warning", StringComparison.OrdinalIgnoreCase) ||
               level.Equals("Error", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the provided <see cref="AnalyzerSettings"/> instance is valid.
    /// </summary>
    /// <param name="value">The settings to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this AnalyzerSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the provided <see cref="AnalyzerSettings"/> instance is valid.
    /// Throws an <see cref="ArgumentException"/> with a detailed message listing all validation errors.
    /// </summary>
    /// <param name="value">The settings to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the settings are invalid.</exception>
    public static void EnsureValid(this AnalyzerSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException($"AnalyzerSettings validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}