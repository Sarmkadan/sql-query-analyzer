#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Extension methods for <see cref="ExplainPlanParserService"/> that provide convenient parsing operations
/// </summary>
public static class ExplainPlanParserServiceExtensions
{
    /// <summary>
    /// Parses a SQL Server execution plan from XML format and returns the query plan
    /// </summary>
    /// <param name="service">The parser service instance</param>
    /// <param name="xmlPlan">The SQL Server XML execution plan</param>
    /// <returns>The parsed query plan</returns>
    /// <exception cref="ArgumentNullException">Thrown when xmlPlan is null or empty</exception>
    public static async Task<QueryPlan> ParseSqlServerPlanAsync(this ExplainPlanParserService service, string xmlPlan)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(xmlPlan, nameof(xmlPlan));

        return await service.ParseSqlServerPlanAsync(xmlPlan).ConfigureAwait(false);
    }

    /// <summary>
    /// Parses a PostgreSQL EXPLAIN plan from JSON format and returns the query plan
    /// </summary>
    /// <param name="service">The parser service instance</param>
    /// <param name="jsonPlan">The PostgreSQL EXPLAIN (FORMAT JSON) output</param>
    /// <returns>The parsed query plan</returns>
    /// <exception cref="ArgumentNullException">Thrown when jsonPlan is null or empty</exception>
    public static async Task<QueryPlan> ParsePostgreSqlPlanAsync(this ExplainPlanParserService service, string jsonPlan)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(jsonPlan, nameof(jsonPlan));

        return await service.ParsePostgreSqlPlanAsync(jsonPlan).ConfigureAwait(false);
    }

    /// <summary>
    /// Parses a MySQL EXPLAIN plan from JSON or tabular format and returns the query plan
    /// </summary>
    /// <param name="service">The parser service instance</param>
    /// <param name="jsonPlan">The MySQL EXPLAIN output (JSON or tabular format)</param>
    /// <returns>The parsed query plan</returns>
    /// <exception cref="ArgumentNullException">Thrown when jsonPlan is null or empty</exception>
    public static async Task<QueryPlan> ParseMySqlPlanAsync(this ExplainPlanParserService service, string jsonPlan)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(jsonPlan, nameof(jsonPlan));

        return await service.ParseMySqlPlanAsync(jsonPlan).ConfigureAwait(false);
    }

    /// <summary>
    /// Extracts performance metrics from a query plan and returns them as a dictionary
    /// </summary>
    /// <param name="service">The parser service instance</param>
    /// <param name="plan">The query plan to analyze</param>
    /// <returns>A dictionary containing performance metrics and analysis</returns>
    /// <exception cref="ArgumentNullException">Thrown when plan is null</exception>
    public static async Task<Dictionary<string, object>> ExtractPlanMetricsAsync(this ExplainPlanParserService service, QueryPlan plan)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(plan);

        return await service.ExtractPlanMetricsAsync(plan).ConfigureAwait(false);
    }

    /// <summary>
    /// Parses a query plan and extracts a simplified performance summary
    /// </summary>
    /// <param name="service">The parser service instance</param>
    /// <param name="plan">The query plan to analyze</param>
    /// <returns>A dictionary containing key performance indicators</returns>
    /// <exception cref="ArgumentNullException">Thrown when plan is null</exception>
    public static async Task<Dictionary<string, object>> GetPlanSummaryAsync(this ExplainPlanParserService service, QueryPlan plan)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(plan);

        var metrics = await service.ExtractPlanMetricsAsync(plan).ConfigureAwait(false);

        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["database"] = plan.DatabaseName ?? "Unknown",
            ["format"] = plan.Format.ToString(),
            ["totalCost"] = metrics["totalCost"],
            ["estimatedRows"] = metrics["estimatedRows"],
            ["elapsedTimeMs"] = metrics["elapsedTime"],
            ["efficiency"] = metrics["efficiency"],
            ["bottlenecks"] = metrics["bottlenecks"],
            ["nodeCount"] = metrics["nodeCount"],
            ["capturedAt"] = plan.CapturedAt
        };
    }

    /// <summary>
    /// Determines if a query plan has performance issues based on common bottlenecks
    /// </summary>
    /// <param name="service">The parser service instance</param>
    /// <param name="plan">The query plan to analyze</param>
    /// <returns>True if performance issues are detected; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when plan is null</exception>
    public static async Task<bool> HasPerformanceIssuesAsync(this ExplainPlanParserService service, QueryPlan plan)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(plan);

        var metrics = await service.ExtractPlanMetricsAsync(plan).ConfigureAwait(false);
        var bottlenecks = (List<string>)metrics["bottlenecks"];

        // Consider a plan to have performance issues if:
        // 1. Efficiency is below 10 (on our 0-100 scale)
        // 2. There are table scans
        // 3. Elapsed time exceeds 1 second
        var efficiency = (double)metrics["efficiency"];
        var elapsedTime = (double)metrics["elapsedTime"];
        var tableScans = bottlenecks.Exists(b => b.Contains("table scans", StringComparison.OrdinalIgnoreCase));

        return efficiency < 10 || tableScans || elapsedTime > 1000;
    }

    /// <summary>
    /// Gets the most expensive operations in the query plan
    /// </summary>
    /// <param name="service">The parser service instance</param>
    /// <param name="plan">The query plan to analyze</param>
    /// <param name="count">Maximum number of expensive operations to return</param>
    /// <returns>List of the most expensive operations</returns>
    /// <exception cref="ArgumentNullException">Thrown when plan is null</exception>
    public static async Task<IReadOnlyList<PlanNode>> GetMostExpensiveOperationsAsync(
        this ExplainPlanParserService service,
        QueryPlan plan,
        int count = 5)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(count, 0);

        var metrics = await service.ExtractPlanMetricsAsync(plan).ConfigureAwait(false);
        var bottlenecks = (List<string>)metrics["bottlenecks"];

        var expensiveOps = new List<PlanNode>();
        foreach (var bottleneck in bottlenecks)
        {
            // Parse bottleneck string to extract node information
            if (bottleneck.Contains(" on ", StringComparison.Ordinal) &&
                bottleneck.Contains("(cost: ", StringComparison.Ordinal))
            {
                var parts = bottleneck.Split(new[] { " on ", " (cost: " },
                    StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 3)
                {
                    var nodeType = parts[0].Trim();
                    var objectName = parts[1].Trim();
                    var costStr = parts[2].TrimEnd(')');

                    if (double.TryParse(costStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var cost))
                    {
                        var node = plan.AllNodes.FirstOrDefault(n =>
                            string.Equals(n.NodeType, nodeType, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(n.ObjectName, objectName, StringComparison.OrdinalIgnoreCase) &&
                            Math.Abs(n.EstimatedCost - cost) < 0.01);

                        if (node != null)
                        {
                            expensiveOps.Add(node);
                            if (expensiveOps.Count >= count)
                                break;
                        }
                    }
                }
            }
        }

        return expensiveOps.AsReadOnly();
    }
}