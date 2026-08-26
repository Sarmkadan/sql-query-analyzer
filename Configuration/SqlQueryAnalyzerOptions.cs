using System.ComponentModel.DataAnnotations;

namespace SqlQueryAnalyzer.Configuration;

/// <summary>
/// Configuration options for the SQL Query Analyzer application.
/// Defines the main configuration structure including database, analysis, cache, performance, and logging settings.
/// </summary>
public class SqlQueryAnalyzerOptions
{
	/// <summary>
	/// Configuration section name used in appsettings.json.
	/// </summary>
	public const string SectionName = "SqlQueryAnalyzer";

	/// <summary>
	/// Database connection and provider configuration options.
	/// </summary>
	[Required]
	public DatabaseOptions Database { get; set; } = new();

	/// <summary>
	/// Analysis and detection behavior configuration options.
	/// </summary>
	[Required]
	public AnalysisOptions Analysis { get; set; } = new();

	/// <summary>
	/// Caching mechanism and performance configuration options.
	/// </summary>
	[Required]
	public CacheOptions Cache { get; set; } = new();

	/// <summary>
	/// Performance tuning and resource management configuration options.
	/// </summary>
	[Required]
	public PerformanceOptions Performance { get; set; } = new();

	/// <summary>
	/// Logging and output configuration options.
	/// </summary>
	[Required]
	public LoggingOptions Logging { get; set; } = new();

    public override string ToString() => $"SqlQueryAnalyzerOptions {{ Database = {Database}, Analysis = {Analysis}, Cache = {Cache}, Performance = {Performance}, Logging = {Logging}, Provider = {Database.Provider} }}";
}

/// <summary>
/// Database connection configuration options.
/// </summary>
public class DatabaseOptions
{
	/// <summary>
	/// Database provider type (e.g., SqlServer, PostgreSql, MySql).
	/// </summary>
	[Required]
	public string Provider { get; set; } = "SqlServer";

	// No hardcoded sensitive default
	/// <summary>
	/// Database connection string containing server, database, and authentication details.
	/// </summary>
	[Required]
	public string ConnectionString { get; set; } = string.Empty;

	/// <summary>
	/// Maximum number of connections to maintain in the connection pool.
	/// </summary>
	[Range(1, 100)]
	public int ConnectionPoolSize { get; set; } = 10;

	/// <summary>
	/// Maximum time in seconds to wait for a database connection before timing out.
	/// </summary>
	[Range(1, 60)]
	public int ConnectionTimeoutSeconds { get; set; } = 5;

	/// <summary>
	/// Enables detailed connection lifecycle logging for debugging purposes.
	/// </summary>
	public bool EnableConnectionLogging { get; set; } = false;
}

/// <summary>
/// Analysis behavior and detection configuration options.
/// </summary>
public class AnalysisOptions
{
	/// <summary>
	/// Maximum number of concurrent analysis threads to use.
	/// Defaults to the number of available processors.
	/// </summary>
	[Range(1, 1024)]
	public int MaxThreads { get; set; } = Environment.ProcessorCount;

	/// <summary>
	/// Enables detection of N+1 query patterns in the analyzed SQL.
	/// </summary>
	public bool DetectNPlusOne { get; set; } = true;

	/// <summary>
	/// Enables detection of missing index recommendations.
	/// </summary>
	public bool DetectMissingIndexes { get; set; } = true;

	/// <summary>
	/// Enables detection of join-related performance issues.
	/// </summary>
	public bool DetectJoinIssues { get; set; } = true;

	/// <summary>
	/// Enables execution plan analysis for query optimization recommendations.
	/// </summary>
	public bool AnalyzeExecutionPlans { get; set; } = true;

	/// <summary>
	/// Sensitivity threshold for identifying critical issues (0.0 to 1.0).
	/// Higher values make the analyzer more conservative in flagging issues.
	/// </summary>
	[Range(0.0, 1.0)]
	public double CriticalIssueSensitivity { get; set; } = 0.8;

	/// <summary>
	/// Enables detailed logging of analysis process for debugging.
	/// </summary>
	public bool EnableDetailedLogging { get; set; } = false;

	/// <summary>
	/// Severity threshold configuration for index recommendations.
	/// </summary>
	[Required]
	public IndexSeverityThresholdsOptions IndexSeverity { get; set; } = new();

	/// <summary>
	/// List of SQL patterns to ignore during analysis (regex patterns).
	/// </summary>
	public List<string> IgnorePatterns { get; set; } = new();
}

/// <summary>
/// Caching mechanism configuration options.
/// </summary>
public class CacheOptions
{
	/// <summary>
	/// Enables caching functionality for query analysis results.
	/// </summary>
	public bool Enabled { get; set; } = true;

	/// <summary>
	/// Cache provider type (e.g., InMemory, Redis).
	/// </summary>
	[Required]
	public string Provider { get; set; } = "InMemory";

	/// <summary>
	/// Maximum number of entries to store in the cache.
	/// </summary>
	[Range(1, 1000000)]
	public int MaxEntries { get; set; } = 10000;

	/// <summary>
	/// Maximum size of the cache in bytes.
	/// </summary>
	[Range(1024, 1073741824)]
	public int MaxSizeBytes { get; set; } = 1024 * 1024 * 100;

	/// <summary>
	/// Cache entry expiration time in seconds.
	/// </summary>
	[Range(1, 86400)]
	public int ExpirationSeconds { get; set; } = 3600;

	/// <summary>
	/// Redis connection string for distributed caching (used when Provider is Redis).
	/// </summary>
	public string? RedisConnectionString { get; set; }
}

/// <summary>
/// Performance tuning and resource management configuration options.
/// </summary>
public class PerformanceOptions
{
	/// <summary>
	/// Maximum time in seconds to wait for query execution before timing out.
	/// </summary>
	[Range(1, 300)]
	public int TimeoutSeconds { get; set; } = 30;

	/// <summary>
	/// Maximum allowed length of a SQL query in characters.
	/// </summary>
	[Range(1024, 10485760)]
	public int MaxQueryLength { get; set; } = 1024 * 1024;

	/// <summary>
	/// Maximum allowed queries per second to prevent rate limiting issues.
	/// </summary>
	[Range(1, 1000)]
	public int RateLimitQueriesPerSecond { get; set; } = 100;

	/// <summary>
	/// Maximum number of concurrent analysis operations allowed.
	/// </summary>
	[Range(1, 100)]
	public int MaxConcurrentAnalysis { get; set; } = 10;

	/// <summary>
	/// Enables batch processing of queries for improved performance.
	/// </summary>
	public bool EnableBatching { get; set; } = true;

	/// <summary>
	/// Number of queries to process in each batch.
	/// </summary>
	[Range(1, 500)]
	public int BatchSize { get; set; } = 50;
}

/// <summary>
/// Logging and output configuration options.
/// </summary>
public class LoggingOptions
{
	/// <summary>
	/// Minimum log level (e.g., Debug, Information, Warning, Error).
	/// </summary>
	[Required]
	public string MinimumLevel { get; set; } = "Information";

	/// <summary>
	/// Enables logging to console output.
	/// </summary>
	public bool ConsoleLogging { get; set; } = true;

	/// <summary>
	/// Enables logging to file.
	/// </summary>
	public bool FileLogging { get; set; } = false;

	/// <summary>
	/// Path to the log file when FileLogging is enabled.
	/// </summary>
	public string? LogFilePath { get; set; }

	/// <summary>
	/// Maximum log file size in bytes before rotation.
	/// </summary>
	[Range(1024, 104857600)]
	public int LogMaxFileSizeBytes { get; set; } = 1024 * 1024 * 10;

	/// <summary>
	/// Maximum number of backup log files to keep.
	/// </summary>
	[Range(1, 20)]
	public int LogMaxBackupFiles { get; set; } = 5;
}

/// <summary>
/// Severity threshold configuration for index recommendations based on row count and cost.
/// </summary>
public class IndexSeverityThresholdsOptions
{
	/// <summary>
	/// Maximum row count for Info severity level recommendations.
	/// </summary>
	[Range(1, 1000000000)]
	public long InfoMaxRows { get; set; } = 10_000;

	/// <summary>
	/// Maximum row count for Warning severity level recommendations.
	/// </summary>
	[Range(1, 1000000000)]
	public long WarningMaxRows { get; set; } = 1_000_000;

	/// <summary>
	/// Maximum estimated cost for Info severity level recommendations.
	/// </summary>
	[Range(0.1, 1000000.0)]
	public double InfoMaxCost { get; set; } = 10.0;

	/// <summary>
	/// Maximum estimated cost for Warning severity level recommendations.
	/// </summary>
	[Range(0.1, 1000000.0)]
	public double WarningMaxCost { get; set; } = 100.0;
}