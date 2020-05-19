#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Models;
using System.Text.Json;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Parses EXPLAIN PLAN output from various database systems
/// </summary>
public class ExplainPlanParserService : IExplainPlanParserService
{
    private readonly IQueryPlanAnalyzerService _planAnalyzer;
    private readonly ILogger<ExplainPlanParserService> _logger;

    public ExplainPlanParserService(
        IQueryPlanAnalyzerService planAnalyzer,
        ILogger<ExplainPlanParserService> logger)
    {
        _planAnalyzer = planAnalyzer;
        _logger = logger;
    }

    public async Task<QueryPlan> ParseSqlServerPlanAsync(string xmlPlan)
    {
        _logger.LogInformation("Parsing SQL Server execution plan");

        var plan = await _planAnalyzer.ParseExecutionPlanAsync(xmlPlan);
        if (plan == null)
        {
            plan = new QueryPlan { Format = PlanFormat.SqlServer };
        }

        return plan;
    }

    public async Task<QueryPlan> ParsePostgreSqlPlanAsync(string jsonPlan)
    {
        _logger.LogInformation("Parsing PostgreSQL EXPLAIN plan");

        var plan = new QueryPlan
        {
            DatabaseName = "PostgreSQL",
            Format = PlanFormat.PostgreSQL,
            CapturedAt = DateTime.UtcNow
        };

        try
        {
            // Hotfix: Replaced rudimentary string-based parsing with System.Text.Json for robust handling of
            // PostgreSQL EXPLAIN (FORMAT JSON) output, especially for new formats like PostgreSQL 17.
            // Extracts Planning Time, Execution Time, Total Cost, and Actual Total Time.

            using var document = System.Text.Json.JsonDocument.Parse(jsonPlan);
            var root = document.RootElement;

            if (root.ValueKind == System.Text.Json.JsonValueKind.Array && root.GetArrayLength() > 0)
            {
                // Assuming the top-level object contains the overall plan details
                var topLevelPlan = root[0];

                // Try to extract Planning Time from the top level
                if (topLevelPlan.TryGetProperty("Planning Time", out var planningTimeElement) &&
                    planningTimeElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                {
                    plan.TotalEstimatedCpuCost = planningTimeElement.GetDouble();
                }

                // Try to extract Execution Time from the top level
                if (topLevelPlan.TryGetProperty("Execution Time", out var executionTimeElement) &&
                    executionTimeElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                {
                    plan.TotalElapsedTime = TimeSpan.FromMilliseconds(executionTimeElement.GetDouble());
                }

                // If a "Plan" property exists, try to extract more details from it
                if (topLevelPlan.TryGetProperty("Plan", out var planElement) &&
                    planElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    // Extract Total Cost if available from the nested "Plan" object
                    if (planElement.TryGetProperty("Total Cost", out var totalCostElement) &&
                        totalCostElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        plan.TotalEstimatedCost = totalCostElement.GetDouble();
                    }

                    // Extract Actual Total Time if available from the nested "Plan" object
                    if (planElement.TryGetProperty("Actual Total Time", out var actualTotalTimeElement) &&
                        actualTotalTimeElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        // Prioritize Actual Total Time for TotalElapsedTime if available and not already set
                        if (plan.TotalElapsedTime == TimeSpan.Zero)
                        {
                            plan.TotalElapsedTime = TimeSpan.FromMilliseconds(actualTotalTimeElement.GetDouble());
                        }
                    }

                    // Extract Plan Rows
                    if (planElement.TryGetProperty("Plan Rows", out var planRowsElement) &&
                        planRowsElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        plan.TotalEstimatedRows = (int)planRowsElement.GetDouble();
                    }
                }
            }

            plan.Initialize();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing PostgreSQL plan");
        }

        return await Task.FromResult(plan);
    }

    public async Task<QueryPlan> ParseMySqlPlanAsync(string jsonPlan)
    {
        _logger.LogInformation("Parsing MySQL EXPLAIN plan");

        var plan = new QueryPlan
        {
            DatabaseName = "MySQL",
            Format = PlanFormat.MySql,
            CapturedAt = DateTime.UtcNow
        };

        try
        {
            var trimmed = jsonPlan.AsSpan().TrimStart();
            if (trimmed.StartsWith("{") || trimmed.StartsWith("["))
                ParseMySqlJsonFormat(plan, jsonPlan);
            else
                ParseMySqlTabularFormat(plan, jsonPlan);

            plan.Initialize();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing MySQL plan");
        }

        return await Task.FromResult(plan);
    }

    /// <summary>
    /// Parses MySQL EXPLAIN FORMAT=JSON output.
    /// Extracts cost_info, used_columns, access types, and nested join details
    /// for accurate index recommendations.
    /// </summary>
    private void ParseMySqlJsonFormat(QueryPlan plan, string jsonPlan)
    {
        using var document = JsonDocument.Parse(jsonPlan);
        var root = document.RootElement;

        // MySQL FORMAT=JSON wraps everything in a top-level "query_block" object.
        var queryBlock = root.ValueKind == JsonValueKind.Object &&
                         root.TryGetProperty("query_block", out var qb) ? qb : root;

        // Top-level query cost
        if (queryBlock.TryGetProperty("cost_info", out var topCostInfo) &&
            topCostInfo.TryGetProperty("query_cost", out var queryCostEl) &&
            double.TryParse(queryCostEl.GetString(),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var queryCost))
        {
            plan.TotalEstimatedCost = queryCost;
        }

        var nodes = new List<PlanNode>();
        ExtractMySqlJsonTables(queryBlock, nodes, plan);
        plan.RootNode = nodes.Count > 0 ? nodes[0] : null;
    }

    /// <summary>
    /// Recursively extracts table nodes from a MySQL EXPLAIN FORMAT=JSON element.
    /// Handles both direct "table" entries and "nested_loop" arrays.
    /// </summary>
    private void ExtractMySqlJsonTables(JsonElement element, List<PlanNode> nodes, QueryPlan plan)
    {
        // Direct table entry
        if (element.TryGetProperty("table", out var tableEl))
        {
            var node = new PlanNode();

            if (tableEl.TryGetProperty("table_name", out var tn))
                node.ObjectName = tn.GetString() ?? string.Empty;

            if (tableEl.TryGetProperty("access_type", out var at))
                node.NodeType = MapMySqlAccessTypeToNodeType(at.GetString() ?? string.Empty);

            if (tableEl.TryGetProperty("rows_examined_per_scan", out var rows) &&
                rows.ValueKind == JsonValueKind.Number)
                node.EstimatedRows = rows.GetInt32();

            if (tableEl.TryGetProperty("cost_info", out var ci) &&
                ci.TryGetProperty("prefix_cost", out var pc) &&
                double.TryParse(pc.GetString(),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var prefixCost))
                node.EstimatedCost = prefixCost;

            // used_columns enables covering-index opportunity detection
            if (tableEl.TryGetProperty("used_columns", out var usedCols) &&
                usedCols.ValueKind == JsonValueKind.Array)
            {
                var cols = new List<string>();
                foreach (var col in usedCols.EnumerateArray())
                    cols.Add(col.GetString() ?? string.Empty);
                node.Properties["used_columns"] = string.Join(", ", cols);
            }

            // possible_keys and key used for unused-index detection
            if (tableEl.TryGetProperty("possible_keys", out var possibleKeys) &&
                possibleKeys.ValueKind == JsonValueKind.Array)
            {
                var keys = new List<string>();
                foreach (var key in possibleKeys.EnumerateArray())
                    keys.Add(key.GetString() ?? string.Empty);
                node.Properties["possible_keys"] = string.Join(", ", keys);
            }

            if (tableEl.TryGetProperty("key", out var keyUsed))
                node.Properties["key"] = keyUsed.GetString() ?? string.Empty;

            plan.TotalEstimatedRows += node.EstimatedRows;
            nodes.Add(node);
        }

        // nested_loop appears when multiple tables are joined
        if (element.TryGetProperty("nested_loop", out var nestedLoop) &&
            nestedLoop.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in nestedLoop.EnumerateArray())
                ExtractMySqlJsonTables(item, nodes, plan);
        }
    }

    private static string MapMySqlAccessTypeToNodeType(string accessType) =>
        accessType.ToUpperInvariant() switch
        {
            "ALL"    => "Table Scan",
            "INDEX"  => "Index Scan",
            "RANGE"  => "Index Range Scan",
            "REF"    => "Index Seek",
            "EQ_REF" => "Index Seek",
            "CONST"  => "Index Seek",
            "SYSTEM" => "Index Seek",
            _        => accessType
        };

    /// <summary>
    /// Fallback heuristic for MySQL default tabular EXPLAIN format.
    /// </summary>
    private static void ParseMySqlTabularFormat(QueryPlan plan, string textPlan)
    {
        var accessTypes = new[] { "system", "const", "eq_ref", "ref", "range", "index", "ALL" };
        foreach (var accessType in accessTypes)
        {
            if (textPlan.Contains(accessType, StringComparison.OrdinalIgnoreCase))
            {
                plan.TotalEstimatedCost += accessType switch
                {
                    "ALL"    => 100.0,
                    "index"  => 50.0,
                    "range"  => 30.0,
                    "ref"    => 10.0,
                    "eq_ref" => 5.0,
                    "const"  => 1.0,
                    "system" => 0.5,
                    _        => 50.0
                };
            }
        }
    }

    public async Task<Dictionary<string, object>> ExtractPlanMetricsAsync(QueryPlan plan)
    {
        // Fix: Added validation to handle null query plan edge case and prevent NullReferenceException
        if (plan == null)
            throw new ArgumentNullException(nameof(plan), "The query plan cannot be null when extracting metrics.");

        _logger.LogInformation("Extracting plan metrics");

        var metrics = new Dictionary<string, object>
        {
            { "totalCost", plan.TotalEstimatedCost },
            { "totalIoCost", plan.TotalEstimatedIoCost },
            { "totalCpuCost", plan.TotalEstimatedCpuCost },
            { "estimatedRows", plan.TotalEstimatedRows },
            { "elapsedTime", plan.TotalElapsedTime.TotalMilliseconds },
            { "logicalReads", plan.TotalLogicalReads },
            { "physicalReads", plan.TotalPhysicalReads },
            { "format", plan.Format.ToString() },
            { "nodeCount", plan.AllNodes.Count },
            { "tableAccessCount", plan.TableAccesses.Count },
            { "joinCount", plan.Joins.Count }
        };

        // Calculate efficiency metrics
        var efficiency = CalculateEfficiency(plan);
        metrics.Add("efficiency", efficiency);

        // Identify bottlenecks
        var bottlenecks = IdentifyBottlenecks(plan);
        metrics.Add("bottlenecks", bottlenecks);

        return await Task.FromResult(metrics);
    }

    private double CalculateEfficiency(QueryPlan plan)
    {
        // Efficiency = TotalRows / TotalCost
        // Higher is better
        if (plan.TotalEstimatedCost == 0)
            return 0;

        var efficiency = (plan.TotalEstimatedRows + 1) / (plan.TotalEstimatedCost + 1);
        return Math.Min(100, efficiency * 10); // Normalize to 0-100
    }

    private List<string> IdentifyBottlenecks(QueryPlan plan)
    {
        var bottlenecks = new List<string>();

        // Find expensive operations
        var expensiveOps = plan.GetExpensiveOperations(3);
        foreach (var op in expensiveOps)
        {
            bottlenecks.Add($"{op.NodeType} on {op.ObjectName} (cost: {op.EstimatedCost:F2})");
        }

        // Identify table scans
        var tableScans = plan.GetTableScans();
        if (tableScans.Count > 0)
        {
            bottlenecks.Add($"Found {tableScans.Count} table scans - consider indexing");
        }

        return bottlenecks;
    }
}
