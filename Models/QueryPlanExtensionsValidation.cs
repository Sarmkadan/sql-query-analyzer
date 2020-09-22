#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides validation helpers for QueryPlanExtensions extension methods
/// </summary>
public static class QueryPlanExtensionsValidation
{
    /// <summary>
    /// Validates the return values from QueryPlanExtensions extension methods
    /// </summary>
    /// <param name="plan">The query plan to validate extension method results against</param>
    /// <returns>List of validation problems (empty if all extension method results are valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null</exception>
    public static IReadOnlyList<string> ValidateQueryPlanExtensions(this QueryPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var problems = new List<string>();

        // Test GetCostPercentage
        try
        {
            var node = new PlanNode { EstimatedCost = 100 };
            var result = plan.GetCostPercentage(node);
            if (double.IsNaN(result) || double.IsInfinity(result))
            {
                problems.Add("GetCostPercentage returned NaN or Infinity");
            }
            if (result < 0 || result > 100)
            {
                problems.Add("GetCostPercentage returned value outside valid range [0, 100]");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetCostPercentage threw exception: {ex.Message}");
        }

        // Test GetNodesAboveThreshold
        try
        {
            var result = plan.GetNodesAboveThreshold(0);
            if (result == null)
            {
                problems.Add("GetNodesAboveThreshold returned null");
            }
            else if (result is not IEnumerable<PlanNode>)
            {
                problems.Add("GetNodesAboveThreshold did not return IEnumerable<PlanNode>");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetNodesAboveThreshold threw exception: {ex.Message}");
        }

        // Test CalculateCumulativeCost
        try
        {
            var result = plan.CalculateCumulativeCost();
            if (double.IsNaN(result) || double.IsInfinity(result))
            {
                problems.Add("CalculateCumulativeCost returned NaN or Infinity");
            }
            if (result < 0)
            {
                problems.Add("CalculateCumulativeCost returned negative value");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"CalculateCumulativeCost threw exception: {ex.Message}");
        }

        // Test GetMostExpensiveTableAccess
        try
        {
            var result = plan.GetMostExpensiveTableAccess();
            // Null is acceptable if no table accesses exist
            if (result != null && result.GetType().Name != nameof(TableAccess))
            {
                problems.Add("GetMostExpensiveTableAccess did not return TableAccess or null");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetMostExpensiveTableAccess threw exception: {ex.Message}");
        }

        // Test GetMostExpensiveJoin
        try
        {
            var result = plan.GetMostExpensiveJoin();
            // Null is acceptable if no joins exist
            if (result != null && result.GetType().Name != nameof(Join))
            {
                problems.Add("GetMostExpensiveJoin did not return Join or null");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetMostExpensiveJoin threw exception: {ex.Message}");
        }

        // Test HasTableScans
        try
        {
            var _ = plan.HasTableScans();
        }
        catch (Exception ex)
        {
            problems.Add($"HasTableScans threw exception: {ex.Message}");
        }

        // Test GetFilteringNodes
        try
        {
            var result = plan.GetFilteringNodes();
            if (result == null)
            {
                problems.Add("GetFilteringNodes returned null");
            }
            else if (result is not IEnumerable<PlanNode>)
            {
                problems.Add("GetFilteringNodes did not return IEnumerable<PlanNode>");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetFilteringNodes threw exception: {ex.Message}");
        }

        // Test GetCpuToIoCostRatio
        try
        {
            var result = plan.GetCpuToIoCostRatio();
            if (double.IsNaN(result) || double.IsInfinity(result))
            {
                problems.Add("GetCpuToIoCostRatio returned NaN or Infinity");
            }
            if (result < 0)
            {
                problems.Add("GetCpuToIoCostRatio returned negative value");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetCpuToIoCostRatio threw exception: {ex.Message}");
        }

        // Test GetSortingNodes
        try
        {
            var result = plan.GetSortingNodes();
            if (result == null)
            {
                problems.Add("GetSortingNodes returned null");
            }
            else if (result is not IEnumerable<PlanNode>)
            {
                problems.Add("GetSortingNodes did not return IEnumerable<PlanNode>");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetSortingNodes threw exception: {ex.Message}");
        }

        // Test GetPerformanceSummary
        try
        {
            var result = plan.GetPerformanceSummary();
            if (result == null)
            {
                problems.Add("GetPerformanceSummary returned null");
            }
            else if (result is not Dictionary<string, object>)
            {
                problems.Add("GetPerformanceSummary did not return Dictionary<string, object>");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetPerformanceSummary threw exception: {ex.Message}");
        }

        // Test IsEfficient
        try
        {
            var _ = plan.IsEfficient();
        }
        catch (Exception ex)
        {
            problems.Add($"IsEfficient threw exception: {ex.Message}");
        }

        // Test GetNodesForTable
        try
        {
            var result = plan.GetNodesForTable("test");
            if (result == null)
            {
                problems.Add("GetNodesForTable returned null");
            }
            else if (result is not IEnumerable<PlanNode>)
            {
                problems.Add("GetNodesForTable did not return IEnumerable<PlanNode>");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetNodesForTable threw exception: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the QueryPlanExtensions extension method results are valid
    /// </summary>
    /// <param name="plan">The query plan to check extension method results against</param>
    /// <returns>True if all extension method results are valid; otherwise, false</returns>
    public static bool AreQueryPlanExtensionsValid(this QueryPlan plan)
    {
        return plan.ValidateQueryPlanExtensions().Count == 0;
    }

    /// <summary>
    /// Ensures that the QueryPlanExtensions extension method results are valid, throwing an exception if they are not
    /// </summary>
    /// <param name="plan">The query plan to validate extension method results against</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when extension method results contain validation problems</exception>
    public static void EnsureQueryPlanExtensionsAreValid(this QueryPlan plan)
    {
        var problems = plan.ValidateQueryPlanExtensions();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"QueryPlan extension methods validation failed. Problems: {string.Join("; ", problems)}");
        }
    }
}
