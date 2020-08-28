using System;
using System.Collections.Generic;
using System.Globalization;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides validation helpers for <see cref="QueryStatistics"/> instances.
/// </summary>
public static class QueryStatisticsValidation
{
    /// <summary>
    /// Validates a <see cref="QueryStatistics"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The query statistics to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this QueryStatistics value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate execution metrics
        if (value.ExecutionCount < 0)
        {
            problems.Add($"ExecutionCount must be non-negative, but was {value.ExecutionCount}.");
        }

        if (value.TotalExecutionTime < TimeSpan.Zero)
        {
            problems.Add($"TotalExecutionTime must be non-negative, but was {value.TotalExecutionTime}.");
        }

        if (value.MinimumExecutionTime < TimeSpan.Zero)
        {
            problems.Add($"MinimumExecutionTime must be non-negative, but was {value.MinimumExecutionTime}.");
        }

        if (value.MaximumExecutionTime < TimeSpan.Zero)
        {
            problems.Add($"MaximumExecutionTime must be non-negative, but was {value.MaximumExecutionTime}.");
        }

        if (value.TotalExecutionTime != TimeSpan.Zero && value.ExecutionCount > 0)
        {
            var calculatedAverage = TimeSpan.FromTicks(value.TotalExecutionTime.Ticks / value.ExecutionCount);
            if (value.MinimumExecutionTime > calculatedAverage || value.MaximumExecutionTime < calculatedAverage)
            {
                problems.Add("MinimumExecutionTime and MaximumExecutionTime must be within bounds of the average execution time.");
            }
        }

        // Validate I/O metrics
        if (value.TotalLogicalReads < 0)
        {
            problems.Add($"TotalLogicalReads must be non-negative, but was {value.TotalLogicalReads}.");
        }

        if (value.TotalPhysicalReads < 0)
        {
            problems.Add($"TotalPhysicalReads must be non-negative, but was {value.TotalPhysicalReads}.");
        }

        if (value.TotalLogicalWrites < 0)
        {
            problems.Add($"TotalLogicalWrites must be non-negative, but was {value.TotalLogicalWrites}.");
        }

        // Validate row metrics
        if (value.RowsAffected < 0)
        {
            problems.Add($"RowsAffected must be non-negative, but was {value.RowsAffected}.");
        }

        if (value.AverageRowsReturned < 0)
        {
            problems.Add($"AverageRowsReturned must be non-negative, but was {value.AverageRowsReturned}.");
        }

        if (value.MaxRowsReturned < 0)
        {
            problems.Add($"MaxRowsReturned must be non-negative, but was {value.MaxRowsReturned}.");
        }

        if (value.MaxRowsReturned > 0 && value.AverageRowsReturned > value.MaxRowsReturned)
        {
            problems.Add("AverageRowsReturned cannot exceed MaxRowsReturned.");
        }

        // Validate time metrics
        if (value.TotalCpuTime < TimeSpan.Zero)
        {
            problems.Add($"TotalCpuTime must be non-negative, but was {value.TotalCpuTime}.");
        }

        if (value.TotalWaitTime < TimeSpan.Zero)
        {
            problems.Add($"TotalWaitTime must be non-negative, but was {value.TotalWaitTime}.");
        }

        // Validate memory metrics
        if (value.PeakMemoryUsageMB < 0)
        {
            problems.Add($"PeakMemoryUsageMB must be non-negative, but was {value.PeakMemoryUsageMB}.");
        }

        if (value.AverageMemoryUsageMB < 0)
        {
            problems.Add($"AverageMemoryUsageMB must be non-negative, but was {value.AverageMemoryUsageMB}.");
        }

        if (value.AverageMemoryUsageMB > value.PeakMemoryUsageMB)
        {
            problems.Add("AverageMemoryUsageMB cannot exceed PeakMemoryUsageMB.");
        }

        // Validate timestamps
        var defaultDate = default(DateTime);
        if (value.LastCompilationTime == defaultDate)
        {
            problems.Add("LastCompilationTime must be a valid non-default DateTime.");
        }

        if (value.FirstExecution == defaultDate)
        {
            problems.Add("FirstExecution must be a valid non-default DateTime.");
        }

        if (value.LastCompilationTime > DateTime.UtcNow.AddHours(1))
        {
            problems.Add("LastCompilationTime cannot be in the future.");
        }

        if (value.FirstExecution > DateTime.UtcNow.AddHours(1))
        {
            problems.Add("FirstExecution cannot be in the future.");
        }

        // Validate cache key
        if (value.IsCached && string.IsNullOrEmpty(value.CacheKey))
        {
            problems.Add("CacheKey must be non-empty when IsCached is true.");
        }

        // Validate plan handle
        if (value.PlanHandle <= 0)
        {
            problems.Add($"PlanHandle must be positive, but was {value.PlanHandle}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="QueryStatistics"/> instance is valid.
    /// </summary>
    /// <param name="value">The query statistics to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this QueryStatistics value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="QueryStatistics"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The query statistics to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this QueryStatistics value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"QueryStatistics validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}