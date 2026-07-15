using System;
using System.Collections.Generic;
using System.Globalization;

namespace SqlQueryAnalyzer.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="SqlQueryAnalyzerOptionsExtensions"/> extension methods.
/// Validates that extension methods return expected values based on configuration.
/// </summary>
public static class SqlQueryAnalyzerOptionsExtensionsValidation
{
    /// <summary>
    /// Validates that <see cref="SqlQueryAnalyzerOptionsExtensions.IsValid"/> extension method works correctly.
    /// </summary>
    /// <param name="value">The SQL query analyzer options to validate.</param>
    /// <returns>List of validation problems; empty if IsValid works correctly.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> ValidateExtensionMethodIsValid(this SqlQueryAnalyzerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        try
        {
            var result = SqlQueryAnalyzerOptionsExtensions.IsValid(value);

            // IsValid should return true only when all required properties are non-null
            var expectedValid = value.Database is not null
                && value.Analysis is not null
                && value.Cache is not null
                && value.Performance is not null
                && value.Logging is not null
                && !string.IsNullOrWhiteSpace(value.Database.Provider)
                && !string.IsNullOrWhiteSpace(value.Database.ConnectionString);

            if (result != expectedValid)
            {
                problems.Add($"IsValid returned {result}, but expected {expectedValid} for options with Database={(value.Database != null)}, Analysis={(value.Analysis != null)}, Cache={(value.Cache != null)}, Performance={(value.Performance != null)}, Logging={(value.Logging != null)}, Provider={value.Database?.Provider}, ConnectionString={(!string.IsNullOrWhiteSpace(value.Database?.ConnectionString) ? "non-empty" : "empty")}");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"IsValid threw exception: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that <see cref="SqlQueryAnalyzerOptionsExtensions.IsAnalyzerEnabled"/> extension method works correctly.
    /// </summary>
    /// <param name="value">The SQL query analyzer options to validate.</param>
    /// <returns>List of validation problems; empty if IsAnalyzerEnabled works correctly.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> ValidateIsAnalyzerEnabled(this SqlQueryAnalyzerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        try
        {
            var result = value.IsAnalyzerEnabled();

            // IsAnalyzerEnabled should return true only when Cache.Enabled is true and Analysis is not null
            var expectedEnabled = value.Cache?.Enabled == true && value.Analysis is not null;

            if (result != expectedEnabled)
            {
                problems.Add($"IsAnalyzerEnabled returned {result}, but expected {expectedEnabled} for options with Cache.Enabled={(value.Cache?.Enabled)}, Analysis={(value.Analysis != null)}");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"IsAnalyzerEnabled threw exception: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that <see cref="SqlQueryAnalyzerOptionsExtensions.GetNormalizedProvider"/> extension method works correctly.
    /// </summary>
    /// <param name="value">The SQL query analyzer options to validate.</param>
    /// <returns>List of validation problems; empty if GetNormalizedProvider works correctly.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> ValidateGetNormalizedProvider(this SqlQueryAnalyzerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        try
        {
            var result = value.GetNormalizedProvider();

            // GetNormalizedProvider should return lowercase normalized provider name
            var expectedProvider = string.IsNullOrWhiteSpace(value.Database?.Provider)
                ? "sqlserver" // Default provider
                : value.Database.Provider.Trim().ToLowerInvariant();

            if (result != expectedProvider)
            {
                problems.Add($"GetNormalizedProvider returned '{result}', but expected '{expectedProvider}' for Provider={value.Database?.Provider}");
            }

            // Should always return lowercase
            if (result != result.ToLowerInvariant())
            {
                problems.Add($"GetNormalizedProvider should return lowercase provider name, but returned '{result}'");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetNormalizedProvider threw exception: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that <see cref="SqlQueryAnalyzerOptionsExtensions.HasCriticalAnalysisEnabled"/> extension method works correctly.
    /// </summary>
    /// <param name="value">The SQL query analyzer options to validate.</param>
    /// <returns>List of validation problems; empty if HasCriticalAnalysisEnabled works correctly.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> ValidateHasCriticalAnalysisEnabled(this SqlQueryAnalyzerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        try
        {
            var result = value.HasCriticalAnalysisEnabled();

            // HasCriticalAnalysisEnabled should return true if any critical analysis feature is enabled
            var expectedEnabled = value.Analysis?.DetectNPlusOne == true
                || value.Analysis?.DetectMissingIndexes == true
                || value.Analysis?.DetectJoinIssues == true
                || value.Analysis?.AnalyzeExecutionPlans == true;

            if (result != expectedEnabled)
            {
                problems.Add($"HasCriticalAnalysisEnabled returned {result}, but expected {expectedEnabled} for options with DetectNPlusOne={(value.Analysis?.DetectNPlusOne)}, DetectMissingIndexes={(value.Analysis?.DetectMissingIndexes)}, DetectJoinIssues={(value.Analysis?.DetectJoinIssues)}, AnalyzeExecutionPlans={(value.Analysis?.AnalyzeExecutionPlans)}");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"HasCriticalAnalysisEnabled threw exception: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that <see cref="SqlQueryAnalyzerOptionsExtensions.GetConnectionTimeoutMs"/> extension method works correctly.
    /// </summary>
    /// <param name="value">The SQL query analyzer options to validate.</param>
    /// <returns>List of validation problems; empty if GetConnectionTimeoutMs works correctly.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> ValidateGetConnectionTimeoutMs(this SqlQueryAnalyzerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        try
        {
            var result = value.GetConnectionTimeoutMs();

            // GetConnectionTimeoutMs should return ConnectionTimeoutSeconds * 1000
            var expectedTimeout = value.Database?.ConnectionTimeoutSeconds * 1000 ?? 0;

            if (result != expectedTimeout)
            {
                problems.Add($"GetConnectionTimeoutMs returned {result}, but expected {expectedTimeout} for ConnectionTimeoutSeconds={value.Database?.ConnectionTimeoutSeconds}");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetConnectionTimeoutMs threw exception: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that <see cref="SqlQueryAnalyzerOptionsExtensions.GetMaxConcurrentThreads"/> extension method works correctly.
    /// </summary>
    /// <param name="value">The SQL query analyzer options to validate.</param>
    /// <returns>List of validation problems; empty if GetMaxConcurrentThreads works correctly.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> ValidateGetMaxConcurrentThreads(this SqlQueryAnalyzerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        try
        {
            var result = value.GetMaxConcurrentThreads();

            // GetMaxConcurrentThreads should clamp Analysis.MaxThreads between 1 and 100
            var expectedThreads = Math.Clamp(value.Analysis?.MaxThreads ?? 1, 1, 100);

            if (result != expectedThreads)
            {
                problems.Add($"GetMaxConcurrentThreads returned {result}, but expected {expectedThreads} for MaxThreads={value.Analysis?.MaxThreads}");
            }

            // Should always be between 1 and 100
            if (result < 1 || result > 100)
            {
                problems.Add($"GetMaxConcurrentThreads returned {result} which is out of range [1, 100]");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetMaxConcurrentThreads threw exception: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that <see cref="SqlQueryAnalyzerOptionsExtensions.ShouldEnableDetailedLogging"/> extension method works correctly.
    /// </summary>
    /// <param name="value">The SQL query analyzer options to validate.</param>
    /// <returns>List of validation problems; empty if ShouldEnableDetailedLogging works correctly.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> ValidateShouldEnableDetailedLogging(this SqlQueryAnalyzerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        try
        {
            var result = value.ShouldEnableDetailedLogging();

            // ShouldEnableDetailedLogging should return Database.EnableConnectionLogging
            var expectedEnabled = value.Database?.EnableConnectionLogging == true;

            if (result != expectedEnabled)
            {
                problems.Add($"ShouldEnableDetailedLogging returned {result}, but expected {expectedEnabled} for EnableConnectionLogging={value.Database?.EnableConnectionLogging}");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ShouldEnableDetailedLogging threw exception: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that <see cref="SqlQueryAnalyzerOptionsExtensions.GetIgnorePatterns"/> extension method works correctly.
    /// </summary>
    /// <param name="value">The SQL query analyzer options to validate.</param>
    /// <returns>List of validation problems; empty if GetIgnorePatterns works correctly.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> ValidateGetIgnorePatterns(this SqlQueryAnalyzerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        try
        {
            var result = value.GetIgnorePatterns();

            // GetIgnorePatterns should return Analysis.IgnorePatterns as IReadOnlyList<string>
            var expectedPatterns = value.Analysis?.IgnorePatterns?.AsReadOnly() ?? Array.Empty<string>().AsReadOnly();

            if (result.Count != expectedPatterns.Count)
            {
                problems.Add($"GetIgnorePatterns returned {result.Count} patterns, but expected {expectedPatterns.Count} patterns");
            }

            // Verify the patterns match
            for (int i = 0; i < Math.Min(result.Count, expectedPatterns.Count); i++)
            {
                if (result[i] != expectedPatterns[i])
                {
                    problems.Add($"GetIgnorePatterns pattern at index {i} returned '{result[i]}', but expected '{expectedPatterns[i]}'");
                }
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetIgnorePatterns threw exception: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that <see cref="SqlQueryAnalyzerOptionsExtensions.ShouldAnalyzeExecutionPlans"/> extension method works correctly.
    /// </summary>
    /// <param name="value">The SQL query analyzer options to validate.</param>
    /// <returns>List of validation problems; empty if ShouldAnalyzeExecutionPlans works correctly.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> ValidateShouldAnalyzeExecutionPlans(this SqlQueryAnalyzerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        try
        {
            var result = value.ShouldAnalyzeExecutionPlans();

            // ShouldAnalyzeExecutionPlans should return Analysis.AnalyzeExecutionPlans
            var expectedEnabled = value.Analysis?.AnalyzeExecutionPlans == true;

            if (result != expectedEnabled)
            {
                problems.Add($"ShouldAnalyzeExecutionPlans returned {result}, but expected {expectedEnabled} for AnalyzeExecutionPlans={value.Analysis?.AnalyzeExecutionPlans}");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ShouldAnalyzeExecutionPlans threw exception: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that <see cref="SqlQueryAnalyzerOptionsExtensions.GetMaxQueryLength"/> extension method works correctly.
    /// </summary>
    /// <param name="value">The SQL query analyzer options to validate.</param>
    /// <returns>List of validation problems; empty if GetMaxQueryLength works correctly.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> ValidateGetMaxQueryLength(this SqlQueryAnalyzerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        try
        {
            var result = value.GetMaxQueryLength();

            // GetMaxQueryLength should return Math.Max(1024, Performance.MaxQueryLength)
            var expectedLength = Math.Max(1024, value.Performance?.MaxQueryLength ?? 1024);

            if (result != expectedLength)
            {
                problems.Add($"GetMaxQueryLength returned {result}, but expected {expectedLength} for MaxQueryLength={value.Performance?.MaxQueryLength}");
            }

            // Should always be at least 1024
            if (result < 1024)
            {
                problems.Add($"GetMaxQueryLength returned {result} which is less than minimum 1024");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetMaxQueryLength threw exception: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates all <see cref="SqlQueryAnalyzerOptionsExtensions"/> extension methods work correctly.
    /// Returns a list of all validation problems found.
    /// </summary>
    /// <param name="value">The SQL query analyzer options to validate.</param>
    /// <returns>List of validation problems; empty if all extension methods work correctly.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> ValidateSqlQueryAnalyzerOptionsExtensions(this SqlQueryAnalyzerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        problems.AddRange(value.ValidateExtensionMethodIsValid());
        problems.AddRange(value.ValidateIsAnalyzerEnabled());
        problems.AddRange(value.ValidateGetNormalizedProvider());
        problems.AddRange(value.ValidateHasCriticalAnalysisEnabled());
        problems.AddRange(value.ValidateGetConnectionTimeoutMs());
        problems.AddRange(value.ValidateGetMaxConcurrentThreads());
        problems.AddRange(value.ValidateShouldEnableDetailedLogging());
        problems.AddRange(value.ValidateGetIgnorePatterns());
        problems.AddRange(value.ValidateShouldAnalyzeExecutionPlans());
        problems.AddRange(value.ValidateGetMaxQueryLength());

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether all <see cref="SqlQueryAnalyzerOptionsExtensions"/> extension methods work correctly.
    /// Returns true if all extension methods produce expected results; otherwise false.
    /// </summary>
    /// <param name="value">The SQL query analyzer options to check.</param>
    /// <returns>True if all extension methods work correctly; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool AreSqlQueryAnalyzerOptionsExtensionsValid(this SqlQueryAnalyzerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.ValidateSqlQueryAnalyzerOptionsExtensions().Count == 0;
    }

    /// <summary>
    /// Ensures that all <see cref="SqlQueryAnalyzerOptionsExtensions"/> extension methods work correctly.
    /// Throws an <see cref="ArgumentException"/> with a detailed message listing all validation problems.
    /// </summary>
    /// <param name="value">The SQL query analyzer options to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when extension methods produce unexpected results.</exception>
    public static void EnsureSqlQueryAnalyzerOptionsExtensionsAreValid(this SqlQueryAnalyzerOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.ValidateSqlQueryAnalyzerOptionsExtensions();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"SqlQueryAnalyzerOptionsExtensions validation failed. Problems:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}