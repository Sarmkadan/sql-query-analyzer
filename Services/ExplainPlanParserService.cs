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
            // Simple JSON-based parsing for PostgreSQL EXPLAIN output
            // In production, would use a proper JSON parser
            if (jsonPlan.Contains("\"Planning Time\""))
            {
                // Extract planning and execution times
                var planningMatch = System.Text.RegularExpressions.Regex.Match(
                    jsonPlan, @"""Planning Time"":\s*([\d.]+)");
                if (planningMatch.Success && double.TryParse(planningMatch.Groups[1].Value, out var planningTime))
                {
                    plan.TotalEstimatedCpuCost = planningTime;
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
            // Parse MySQL EXPLAIN JSON format
            // Extract key metrics
            if (jsonPlan.Contains("\"type\""))
            {
                // Analyze access types
                var accessTypes = new[] { "system", "const", "eq_ref", "ref", "range", "index", "ALL" };
                foreach (var accessType in accessTypes)
                {
                    if (jsonPlan.Contains($"\"{accessType}\""))
                    {
                        // Estimate cost based on access type
                        var estimatedCost = accessType switch
                        {
                            "ALL" => 100.0,  // Full table scan
                            "index" => 50.0, // Index scan
                            "range" => 30.0,
                            "ref" => 10.0,
                            "eq_ref" => 5.0,
                            "const" => 1.0,
                            "system" => 0.5,
                            _ => 50.0
                        };
                        plan.TotalEstimatedCost += estimatedCost;
                    }
                }
            }

            plan.Initialize();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing MySQL plan");
        }

        return await Task.FromResult(plan);
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
