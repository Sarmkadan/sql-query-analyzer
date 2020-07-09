#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace SqlQueryAnalyzer.CLI;

/// <summary>
/// Provides validation helpers for <see cref="CommandLineArguments"/> instances.
/// </summary>
public static class CommandLineArgumentsValidation
{
    /// <summary>
    /// Validates the specified <see cref="CommandLineArguments"/> instance.
    /// </summary>
    /// <param name="value">The command-line arguments to validate.</param>
    /// <returns>A read-only list of validation problems; empty if validation succeeds.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CommandLineArguments value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Check required arguments
        if (!value.ShowHelp && !value.ShowVersion)
        {
            if (string.IsNullOrWhiteSpace(value.Query) && string.IsNullOrWhiteSpace(value.QueryFile))
            {
                problems.Add("Either --query or --query-file must be provided when not showing help or version");
            }
        }

        // Validate Query
        if (!string.IsNullOrWhiteSpace(value.Query) && string.IsNullOrWhiteSpace(value.Query.Trim()))
        {
            problems.Add("Query cannot be empty or whitespace");
        }

        // Validate QueryFile
        if (!string.IsNullOrWhiteSpace(value.QueryFile))
        {
            if (string.IsNullOrWhiteSpace(value.QueryFile.Trim()))
            {
                problems.Add("Query file path cannot be empty or whitespace");
            }
            else if (!Path.IsPathRooted(value.QueryFile) && !Path.Exists(value.QueryFile))
            {
                // Note: We don't validate file existence here as it's checked later
                // Just ensure the path format is reasonable
                var path = value.QueryFile.Trim();
                if (path.Contains("..") || path.StartsWith("/") || path.StartsWith("\\"))
                {
                    problems.Add("Query file path contains invalid characters or path traversal sequences");
                }
            }
        }

        // Validate OutputFormat
        if (!string.IsNullOrWhiteSpace(value.OutputFormat))
        {
            var format = value.OutputFormat.Trim().ToLowerInvariant();
            if (!new[] { "json", "csv", "xml", "html", "text" }.Contains(format))
            {
                problems.Add($"Invalid output format '{value.OutputFormat}'. Supported formats: json, csv, xml, html, text");
            }
        }

        // Validate OutputPath
        if (!string.IsNullOrWhiteSpace(value.OutputPath))
        {
            var path = value.OutputPath.Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                problems.Add("Output path cannot be empty or whitespace");
            }
            else if (path.Contains("..") || path.StartsWith("/") || path.StartsWith("\\"))
            {
                problems.Add("Output path contains invalid characters or path traversal sequences");
            }
        }

        // Validate DatabaseConnection
        if (!string.IsNullOrWhiteSpace(value.DatabaseConnection))
        {
            var connection = value.DatabaseConnection.Trim();
            if (string.IsNullOrWhiteSpace(connection))
            {
                problems.Add("Database connection string cannot be empty or whitespace");
            }
            else if (connection.Length < 5) // Minimum reasonable connection string
            {
                problems.Add("Database connection string is too short to be valid");
            }
        }

        // Validate ConfigFile
        if (!string.IsNullOrWhiteSpace(value.ConfigFile))
        {
            var configPath = value.ConfigFile.Trim();
            if (string.IsNullOrWhiteSpace(configPath))
            {
                problems.Add("Config file path cannot be empty or whitespace");
            }
            else if (configPath.Contains("..") || configPath.StartsWith("/") || configPath.StartsWith("\\"))
            {
                problems.Add("Config file path contains invalid characters or path traversal sequences");
            }
        }

        // Validate ThreadCount
        if (value.ThreadCount < 1)
        {
            problems.Add("Thread count must be at least 1");
        }
        else if (value.ThreadCount > Environment.ProcessorCount * 4) // Reasonable upper bound
        {
            problems.Add($"Thread count {value.ThreadCount} exceeds reasonable maximum of {Environment.ProcessorCount * 4}");
        }

        // Validate SqlServerVersion
        if (!string.IsNullOrWhiteSpace(value.SqlServerVersion))
        {
            var version = value.SqlServerVersion.Trim();
            if (string.IsNullOrWhiteSpace(version))
            {
                problems.Add("SQL Server version cannot be empty or whitespace");
            }
            else if (!version.StartsWith("20", StringComparison.Ordinal) && version != "latest")
            {
                problems.Add("SQL Server version must start with '20' (e.g., '2019', '2022') or be 'latest'");
            }
        }

        // Validate FilterBySeverity
        if (!string.IsNullOrWhiteSpace(value.FilterBySeverity))
        {
            var severity = value.FilterBySeverity.Trim();
            if (!new[] { "Critical", "Warning", "Info" }.Contains(severity))
            {
                problems.Add($"Invalid severity filter '{value.FilterBySeverity}'. Supported: Critical, Warning, Info");
            }
        }

        // Validate MaxResults
        if (value.MaxResults.HasValue)
        {
            if (value.MaxResults.Value < 1)
            {
                problems.Add("Max results must be at least 1 if specified");
            }
            else if (value.MaxResults.Value > 1000000) // 1 million is a reasonable upper bound
            {
                problems.Add($"Max results {value.MaxResults.Value} exceeds reasonable maximum of 1,000,000");
            }
        }

        // Validate CachePath
        if (!string.IsNullOrWhiteSpace(value.CachePath))
        {
            var cachePath = value.CachePath.Trim();
            if (string.IsNullOrWhiteSpace(cachePath))
            {
                problems.Add("Cache path cannot be empty or whitespace");
            }
            else if (cachePath.Contains("..") || cachePath.StartsWith("/") || cachePath.StartsWith("\\"))
            {
                problems.Add("Cache path contains invalid characters or path traversal sequences");
            }
        }

        // Validate SlowLogFile
        if (!string.IsNullOrWhiteSpace(value.SlowLogFile))
        {
            var logPath = value.SlowLogFile.Trim();
            if (string.IsNullOrWhiteSpace(logPath))
            {
                problems.Add("Slow log file path cannot be empty or whitespace");
            }
            else if (logPath.Contains("..") || logPath.StartsWith("/") || logPath.StartsWith("\\"))
            {
                problems.Add("Slow log file path contains invalid characters or path traversal sequences");
            }
        }

        // Validate SlowLogFormat
        if (!string.IsNullOrWhiteSpace(value.SlowLogFormat))
        {
            var format = value.SlowLogFormat.Trim().ToLowerInvariant();
            if (!new[] { "mysql", "postgres", "sqlserver", "oracle" }.Contains(format))
            {
                problems.Add(string.Format(CultureInfo.InvariantCulture, "Invalid slow log format '{0}'. Supported: mysql, postgres, sqlserver, oracle", value.SlowLogFormat));
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="CommandLineArguments"/> instance is valid.
    /// </summary>
    /// <param name="value">The command-line arguments to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this CommandLineArguments value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var validationResult = CommandLineArgumentsValidation.Validate(value);
        return !validationResult.Any();
    }

    /// <summary>
    /// Validates the specified <see cref="CommandLineArguments"/> instance and throws an <see cref="ArgumentException"/> if validation fails.
    /// </summary>
    /// <param name="value">The command-line arguments to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing a list of all validation problems.</exception>
    public static void EnsureValid(this CommandLineArguments value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = CommandLineArgumentsValidation.Validate(value);
        if (problems.Any())
        {
            var message = string.Join("\n- ", problems);
            throw new ArgumentException(
                $"Command line arguments validation failed:\n- {message}");
        }
    }
}
