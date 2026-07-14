using System.Globalization;

namespace SqlQueryAnalyzer.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="SqlQueryAnalyzerOptions"/> configuration.
/// </summary>
public static class SqlQueryAnalyzerOptionsValidation
{
    /// <summary>
    /// Validates the provided <see cref="SqlQueryAnalyzerOptions"/> instance.
    /// Returns a list of human-readable validation errors.
    /// </summary>
    /// <param name="value">The options to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SqlQueryAnalyzerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Database settings
        if (value.Database is null)
        {
            errors.Add("Database settings cannot be null.");
        }
        else
        {
            errors.AddRange(value.Database.Validate());
        }

        // Validate Analysis settings
        if (value.Analysis is null)
        {
            errors.Add("Analysis settings cannot be null.");
        }
        else
        {
            errors.AddRange(value.Analysis.Validate());
        }

        // Validate Cache settings
        if (value.Cache is null)
        {
            errors.Add("Cache settings cannot be null.");
        }
        else
        {
            errors.AddRange(value.Cache.Validate());
        }

        // Validate Performance settings
        if (value.Performance is null)
        {
            errors.Add("Performance settings cannot be null.");
        }
        else
        {
            errors.AddRange(value.Performance.Validate());
        }

        // Validate Logging settings
        if (value.Logging is null)
        {
            errors.Add("Logging settings cannot be null.");
        }
        else
        {
            errors.AddRange(value.Logging.Validate());
        }

        // Note: Properties like Provider, ConnectionString, ConnectionPoolSize, etc.
        // are validated through their respective nested option classes (DatabaseOptions, AnalysisOptions, etc.)

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="DatabaseOptions"/> instance.
    /// </summary>
    /// <param name="value">The database options to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DatabaseOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Provider))
        {
            errors.Add("DatabaseOptions.Provider cannot be null or whitespace.");
        }
        else if (!IsValidProvider(value.Provider))
        {
            errors.Add("DatabaseOptions.Provider must be one of: SqlServer, PostgreSql, MySql.");
        }

        if (string.IsNullOrWhiteSpace(value.ConnectionString))
        {
            errors.Add("DatabaseOptions.ConnectionString cannot be null or whitespace.");
        }

        if (value.ConnectionPoolSize < 1)
        {
            errors.Add("DatabaseOptions.ConnectionPoolSize must be at least 1.");
        }

        if (value.ConnectionTimeoutSeconds < 1)
        {
            errors.Add("DatabaseOptions.ConnectionTimeoutSeconds must be at least 1.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="AnalysisOptions"/> instance.
    /// </summary>
    /// <param name="value">The analysis options to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this AnalysisOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (value.MaxThreads < 1)
        {
            errors.Add("AnalysisOptions.MaxThreads must be at least 1.");
        }

        if (value.CriticalIssueSensitivity < 0 || value.CriticalIssueSensitivity > 1)
        {
            errors.Add("AnalysisOptions.CriticalIssueSensitivity must be between 0 and 1 (inclusive).");
        }

        if (value.IndexSeverity is null)
        {
            errors.Add("AnalysisOptions.IndexSeverity cannot be null.");
        }
        else
        {
            errors.AddRange(value.IndexSeverity.Validate());
        }

        if (value.IgnorePatterns is null)
        {
            errors.Add("AnalysisOptions.IgnorePatterns cannot be null.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="CacheOptions"/> instance.
    /// </summary>
    /// <param name="value">The cache options to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CacheOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Provider))
        {
            errors.Add("CacheOptions.Provider cannot be null or whitespace.");
        }
        else if (!IsValidCacheProvider(value.Provider))
        {
            errors.Add("CacheOptions.Provider must be one of: InMemory, Redis.");
        }

        if (value.MaxEntries < 1)
        {
            errors.Add("CacheOptions.MaxEntries must be at least 1.");
        }

        if (value.MaxSizeBytes < 1024)
        {
            errors.Add("CacheOptions.MaxSizeBytes must be at least 1 KB (1024 bytes).");
        }

        if (value.ExpirationSeconds < 1)
        {
            errors.Add("CacheOptions.ExpirationSeconds must be at least 1.");
        }

        if (value.Provider.Equals("Redis", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(value.RedisConnectionString))
        {
            errors.Add("CacheOptions.RedisConnectionString is required when CacheOptions.Provider is Redis.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="PerformanceOptions"/> instance.
    /// </summary>
    /// <param name="value">The performance options to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PerformanceOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (value.TimeoutSeconds < 1)
        {
            errors.Add("PerformanceOptions.TimeoutSeconds must be at least 1.");
        }

        if (value.MaxQueryLength < 1024)
        {
            errors.Add("PerformanceOptions.MaxQueryLength must be at least 1024 characters.");
        }

        if (value.RateLimitQueriesPerSecond < 1)
        {
            errors.Add("PerformanceOptions.RateLimitQueriesPerSecond must be at least 1.");
        }

        if (value.MaxConcurrentAnalysis < 1)
        {
            errors.Add("PerformanceOptions.MaxConcurrentAnalysis must be at least 1.");
        }

        if (value.BatchSize < 1)
        {
            errors.Add("PerformanceOptions.BatchSize must be at least 1.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="LoggingOptions"/> instance.
    /// </summary>
    /// <param name="value">The logging options to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this LoggingOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(value.MinimumLevel))
        {
            errors.Add("LoggingOptions.MinimumLevel cannot be null or whitespace.");
        }
        else if (!IsValidLogLevel(value.MinimumLevel))
        {
            errors.Add("LoggingOptions.MinimumLevel must be one of: Debug, Information, Warning, Error.");
        }

        if (value.LogMaxFileSizeBytes < 1024)
        {
            errors.Add("LoggingOptions.LogMaxFileSizeBytes must be at least 1 KB (1024 bytes).");
        }

        if (value.LogMaxBackupFiles < 0)
        {
            errors.Add("LoggingOptions.LogMaxBackupFiles must be non-negative.");
        }

        if (value.FileLogging && string.IsNullOrWhiteSpace(value.LogFilePath))
        {
            errors.Add("LoggingOptions.LogFilePath is required when LoggingOptions.FileLogging is true.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="IndexSeverityThresholdsOptions"/> instance.
    /// </summary>
    /// <param name="value">The index severity thresholds to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this IndexSeverityThresholdsOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (value.InfoMaxRows < 1)
        {
            errors.Add("IndexSeverityThresholdsOptions.InfoMaxRows must be at least 1.");
        }

        if (value.WarningMaxRows < 1)
        {
            errors.Add("IndexSeverityThresholdsOptions.WarningMaxRows must be at least 1.");
        }

        if (value.InfoMaxRows > value.WarningMaxRows)
        {
            errors.Add("IndexSeverityThresholdsOptions.InfoMaxRows must be less than or equal to IndexSeverityThresholdsOptions.WarningMaxRows.");
        }

        if (value.InfoMaxCost < 0.1)
        {
            errors.Add("IndexSeverityThresholdsOptions.InfoMaxCost must be at least 0.1.");
        }

        if (value.WarningMaxCost < 0.1)
        {
            errors.Add("IndexSeverityThresholdsOptions.WarningMaxCost must be at least 0.1.");
        }

        if (value.InfoMaxCost > value.WarningMaxCost)
        {
            errors.Add("IndexSeverityThresholdsOptions.InfoMaxCost must be less than or equal to IndexSeverityThresholdsOptions.WarningMaxCost.");
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
               provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase) ||
               provider.Equals("MySql", StringComparison.OrdinalIgnoreCase);
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
    /// Determines whether the provided <see cref="SqlQueryAnalyzerOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SqlQueryAnalyzerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the provided <see cref="SqlQueryAnalyzerOptions"/> instance is valid.
    /// Throws an <see cref="ArgumentException"/> with a detailed message listing all validation errors.
    /// </summary>
    /// <param name="value">The options to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the options are invalid.</exception>
    public static void EnsureValid(this SqlQueryAnalyzerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"SqlQueryAnalyzerOptions validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}