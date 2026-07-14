#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides validation helpers for <see cref="QueryPlan"/> instances
/// </summary>
public static class QueryPlanValidation
{
    /// <summary>
    /// Validates a <see cref="QueryPlan"/> instance and returns a list of human-readable problems
    /// </summary>
    /// <param name="value">The query plan to validate</param>
    /// <returns>List of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this QueryPlan value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate PlanId
        if (string.IsNullOrWhiteSpace(value.PlanId))
        {
            problems.Add("PlanId is null or empty");
        }

        // Validate DatabaseName
        if (string.IsNullOrWhiteSpace(value.DatabaseName))
        {
            problems.Add("DatabaseName is null or empty");
        }

        // Validate CapturedAt (should not be default DateTime)
        if (value.CapturedAt == default)
        {
            problems.Add("CapturedAt has not been set (default DateTime)");
        }
        else if (value.CapturedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("CapturedAt is in the future");
        }

        // Validate Format
        if (value.Format == PlanFormat.Unknown)
        {
            problems.Add("Format is set to Unknown");
        }

        // Validate RootNode
        if (value.RootNode == null)
        {
            problems.Add("RootNode is null");
        }

        // Validate cost metrics (should be non-negative)
        if (value.TotalEstimatedCost < 0)
        {
            problems.Add("TotalEstimatedCost is negative");
        }

        if (value.TotalEstimatedIoCost < 0)
        {
            problems.Add("TotalEstimatedIoCost is negative");
        }

        if (value.TotalEstimatedCpuCost < 0)
        {
            problems.Add("TotalEstimatedCpuCost is negative");
        }

        // Validate row estimates
        if (value.TotalEstimatedRows < 0)
        {
            problems.Add("TotalEstimatedRows is negative");
        }

        // Validate time metrics
        if (value.TotalElapsedTime < TimeSpan.Zero)
        {
            problems.Add("TotalElapsedTime is negative");
        }

        if (value.TotalLogicalReads < 0)
        {
            problems.Add("TotalLogicalReads is negative");
        }

        if (value.TotalPhysicalReads < 0)
        {
            problems.Add("TotalPhysicalReads is negative");
        }

        // Validate collection properties
        if (value.AllNodes == null)
        {
            problems.Add("AllNodes collection is null");
        }

        if (value.TableAccesses == null)
        {
            problems.Add("TableAccesses collection is null");
        }

        if (value.Joins == null)
        {
            problems.Add("Joins collection is null");
        }

        // Validate that RootNode is in AllNodes if both are set
        if (value.RootNode != null && value.AllNodes != null && !value.AllNodes.Contains(value.RootNode))
        {
            problems.Add("RootNode is not present in AllNodes collection");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="QueryPlan"/> is valid
    /// </summary>
    /// <param name="value">The query plan to check</param>
    /// <returns>True if the query plan is valid; otherwise, false</returns>
    public static bool IsValid(this QueryPlan value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="QueryPlan"/> is valid, throwing an exception if it is not
    /// </summary>
    /// <param name="value">The query plan to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when the query plan contains validation problems</exception>
    public static void EnsureValid(this QueryPlan value)
    {
        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"QueryPlan is invalid. Problems: {string.Join("; ", problems)}");
        }
    }
}
