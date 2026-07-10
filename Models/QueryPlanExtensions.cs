#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Extension methods for QueryPlan providing additional analysis and utility functions
/// </summary>
public static class QueryPlanExtensions
{
    /// <summary>
    /// Calculates the total cost percentage of a specific node relative to the total plan cost
    /// </summary>
    /// <param name="plan">The query plan</param>
    /// <param name="node">The plan node to analyze</param>
    /// <returns>Cost percentage (0-100)</returns>
    public static double GetCostPercentage(this QueryPlan plan, PlanNode node)
    {
        if (plan.TotalEstimatedCost <= 0 || node.EstimatedCost <= 0)
            return 0;

        return Math.Round((node.EstimatedCost / plan.TotalEstimatedCost) * 100, 2);
    }

    /// <summary>
    /// Gets all nodes that exceed a specified cost threshold
    /// </summary>
    /// <param name="plan">The query plan</param>
    /// <param name="threshold">Minimum cost threshold to include</param>
    /// <returns>List of nodes exceeding the threshold</returns>
    public static List<PlanNode> GetNodesAboveThreshold(this QueryPlan plan, double threshold)
    {
        return plan.AllNodes
            .Where(n => n.EstimatedCost >= threshold)
            .OrderByDescending(n => n.EstimatedCost)
            .ToList();
    }

    /// <summary>
    /// Calculates the cumulative cost of all operations in the plan
    /// </summary>
    /// <param name="plan">The query plan</param>
    /// <returns>Sum of all node costs</returns>
    public static double CalculateCumulativeCost(this QueryPlan plan)
    {
        return plan.AllNodes.Sum(n => n.EstimatedCost);
    }

    /// <summary>
    /// Gets the most expensive table access operation
    /// </summary>
    /// <param name="plan">The query plan</param>
    /// <returns>The table access with highest estimated cost, or null if none</returns>
    public static TableAccess? GetMostExpensiveTableAccess(this QueryPlan plan)
    {
        return plan.TableAccesses
            .OrderByDescending(ta => ta.EstimatedCost)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets the most expensive join operation
    /// </summary>
    /// <param name="plan">The query plan</param>
    /// <returns>The join with highest estimated cost, or null if none</returns>
    public static Join? GetMostExpensiveJoin(this QueryPlan plan)
    {
        return plan.Joins
            .OrderByDescending(j => j.EstimatedCost)
            .FirstOrDefault();
    }

    /// <summary>
    /// Determines if the plan has any table scans (potential performance issue)
    /// </summary>
    /// <param name="plan">The query plan</param>
    /// <returns>True if any table scans exist, false otherwise</returns>
    public static bool HasTableScans(this QueryPlan plan)
    {
        return plan.GetTableScans().Any();
    }

    /// <summary>
    /// Gets all nodes that perform data filtering (WHERE clauses, etc.)
    /// </summary>
    /// <param name="plan">The query plan</param>
    /// <returns>List of filtering nodes</returns>
    public static List<PlanNode> GetFilteringNodes(this QueryPlan plan)
    {
        return plan.AllNodes
            .Where(n => n.NodeType.Contains("Filter") ||
                        n.Properties.ContainsKey("Predicate") ||
                        n.NodeType.Contains("Compute Scalar"))
            .ToList();
    }

    /// <summary>
    /// Calculates the estimated cost ratio between CPU and I/O operations
    /// </summary>
    /// <param name="plan">The query plan</param>
    /// <returns>CPU/I/O cost ratio (CPU cost divided by I/O cost)</returns>
    public static double GetCpuToIoCostRatio(this QueryPlan plan)
    {
        if (plan.TotalEstimatedIoCost <= 0)
            return plan.TotalEstimatedCpuCost > 0 ? double.PositiveInfinity : 0;

        return Math.Round(plan.TotalEstimatedCpuCost / plan.TotalEstimatedIoCost, 2);
    }

    /// <summary>
    /// Gets all nodes that perform sorting operations
    /// </summary>
    /// <param name="plan">The query plan</param>
    /// <returns>List of sorting nodes</returns>
    public static List<PlanNode> GetSortingNodes(this QueryPlan plan)
    {
        return plan.AllNodes
            .Where(n => n.NodeType.Contains("Sort") ||
                        n.NodeType.Contains("Top N Sort") ||
                        n.NodeType.Contains("Stream Aggregate"))
            .ToList();
    }

    /// <summary>
    /// Gets a summary of plan performance characteristics
    /// </summary>
    /// <param name="plan">The query plan</param>
    /// <returns>Dictionary with performance metrics</returns>
    public static Dictionary<string, object> GetPerformanceSummary(this QueryPlan plan)
    {
        var expensiveOps = plan.GetExpensiveOperations(10);
        var tableScans = plan.GetTableScans();
        var indexOps = plan.GetIndexOperations();
        var filteringNodes = plan.GetFilteringNodes();
        var sortingNodes = plan.GetSortingNodes();

        return new Dictionary<string, object>
        {
            { "planId", plan.PlanId },
            { "database", plan.DatabaseName },
            { "capturedAt", plan.CapturedAt },
            { "totalCost", plan.TotalEstimatedCost },
            { "cpuCost", plan.TotalEstimatedCpuCost },
            { "ioCost", plan.TotalEstimatedIoCost },
            { "rows", plan.TotalEstimatedRows },
            { "elapsedTimeMs", plan.TotalElapsedTime.TotalMilliseconds },
            { "logicalReads", plan.TotalLogicalReads },
            { "physicalReads", plan.TotalPhysicalReads },
            { "nodeCount", plan.AllNodes.Count },
            { "expensiveOperationsCount", expensiveOps.Count },
            { "tableScansCount", tableScans.Count },
            { "indexOperationsCount", indexOps.Count },
            { "filteringNodesCount", filteringNodes.Count },
            { "sortingNodesCount", sortingNodes.Count },
            { "joinsCount", plan.Joins.Count },
            { "tableAccessesCount", plan.TableAccesses.Count },
            { "hasTableScans", tableScans.Any() },
            { "cpuToIoRatio", plan.GetCpuToIoCostRatio() },
            { "topExpensiveOperations", expensiveOps.Select(n => new {
                nodeType = n.NodeType,
                objectName = n.ObjectName,
                cost = n.EstimatedCost,
                rows = n.EstimatedRows,
                costPercentage = plan.GetCostPercentage(n)
            }).ToList() }
        };
    }

    /// <summary>
    /// Determines if the plan is considered efficient based on various metrics
    /// </summary>
    /// <param name="plan">The query plan</param>
    /// <param name="maxTableScans">Maximum allowed table scans for efficiency</param>
    /// <param name="maxCost">Maximum acceptable total cost</param>
    /// <returns>True if plan is efficient, false otherwise</returns>
    public static bool IsEfficient(this QueryPlan plan, int maxTableScans = 2, double maxCost = 1000.0)
    {
        var tableScans = plan.GetTableScans();
        var expensiveOps = plan.GetNodesAboveThreshold(maxCost / 10);

        return !tableScans.Any() &&
               plan.TotalEstimatedCost <= maxCost &&
               expensiveOps.Count <= 3 &&
               plan.TotalLogicalReads < 100000 &&
               plan.TotalElapsedTime.TotalSeconds < 10;
    }

    /// <summary>
    /// Gets all nodes that access a specific table
    /// </summary>
    /// <param name="plan">The query plan</param>
    /// <param name="tableName">Name of the table to find</param>
    /// <returns>List of nodes accessing the specified table</returns>
    public static List<PlanNode> GetNodesForTable(this QueryPlan plan, string tableName)
    {
        return plan.AllNodes
            .Where(n => string.Equals(n.ObjectName, tableName, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}