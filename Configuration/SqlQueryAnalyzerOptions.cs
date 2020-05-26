using System.ComponentModel.DataAnnotations;

namespace SqlQueryAnalyzer.Configuration;

public class SqlQueryAnalyzerOptions
{
    public const string SectionName = "SqlQueryAnalyzer";

    [Required]
    public DatabaseOptions Database { get; set; } = new();

    [Required]
    public AnalysisOptions Analysis { get; set; } = new();

    [Required]
    public CacheOptions Cache { get; set; } = new();

    [Required]
    public PerformanceOptions Performance { get; set; } = new();

    [Required]
    public LoggingOptions Logging { get; set; } = new();
}

public class DatabaseOptions
{
    [Required]
    public string Provider { get; set; } = "SqlServer";

    // No hardcoded sensitive default
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 100)]
    public int ConnectionPoolSize { get; set; } = 10;

    [Range(1, 60)]
    public int ConnectionTimeoutSeconds { get; set; } = 5;

    public bool EnableConnectionLogging { get; set; } = false;
}

public class AnalysisOptions
{
    [Range(1, 1024)]
    public int MaxThreads { get; set; } = Environment.ProcessorCount;

    public bool DetectNPlusOne { get; set; } = true;
    public bool DetectMissingIndexes { get; set; } = true;
    public bool DetectJoinIssues { get; set; } = true;
    public bool AnalyzeExecutionPlans { get; set; } = true;

    [Range(0.0, 1.0)]
    public double CriticalIssueSensitivity { get; set; } = 0.8;

    public bool EnableDetailedLogging { get; set; } = false;

    [Required]
    public IndexSeverityThresholdsOptions IndexSeverity { get; set; } = new();

    public List<string> IgnorePatterns { get; set; } = new();
}

public class CacheOptions
{
    public bool Enabled { get; set; } = true;
    [Required]
    public string Provider { get; set; } = "InMemory";

    [Range(1, 1000000)]
    public int MaxEntries { get; set; } = 10000;

    [Range(1024, 1073741824)]
    public int MaxSizeBytes { get; set; } = 1024 * 1024 * 100;

    [Range(1, 86400)]
    public int ExpirationSeconds { get; set; } = 3600;

    public string? RedisConnectionString { get; set; }
}

public class PerformanceOptions
{
    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;

    [Range(1024, 10485760)]
    public int MaxQueryLength { get; set; } = 1024 * 1024;

    [Range(1, 1000)]
    public int RateLimitQueriesPerSecond { get; set; } = 100;

    [Range(1, 100)]
    public int MaxConcurrentAnalysis { get; set; } = 10;

    public bool EnableBatching { get; set; } = true;

    [Range(1, 500)]
    public int BatchSize { get; set; } = 50;
}

public class LoggingOptions
{
    [Required]
    public string MinimumLevel { get; set; } = "Information";

    public bool ConsoleLogging { get; set; } = true;
    public bool FileLogging { get; set; } = false;
    public string? LogFilePath { get; set; }

    [Range(1024, 104857600)]
    public int LogMaxFileSizeBytes { get; set; } = 1024 * 1024 * 10;

    [Range(1, 20)]
    public int LogMaxBackupFiles { get; set; } = 5;
}

public class IndexSeverityThresholdsOptions
{
    [Range(1, 1000000000)]
    public long InfoMaxRows { get; set; } = 10_000;

    [Range(1, 1000000000)]
    public long WarningMaxRows { get; set; } = 1_000_000;

    [Range(0.1, 1000000.0)]
    public double InfoMaxCost { get; set; } = 10.0;

    [Range(0.1, 1000000.0)]
    public double WarningMaxCost { get; set; } = 100.0;
}
