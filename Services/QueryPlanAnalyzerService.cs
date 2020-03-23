#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Analyzes SQL Server execution plans for performance issues
/// </summary>
public class QueryPlanAnalyzerService : IQueryPlanAnalyzerService
{
    private readonly ILogger<QueryPlanAnalyzerService> _logger;

    public QueryPlanAnalyzerService(ILogger<QueryPlanAnalyzerService> logger)
    {
        _logger = logger;
    }

    public async Task<QueryPlan?> ParseExecutionPlanAsync(string planXml)
    {
        try
        {
            _logger.LogInformation("Parsing execution plan XML");

            var doc = XDocument.Parse(planXml);
            var plan = new QueryPlan
            {
                Format = PlanFormat.SqlServer,
                CapturedAt = DateTime.UtcNow
            };

            // Parse root node
            var showPlanElement = doc.Descendants("ShowPlanXML").FirstOrDefault();
            if (showPlanElement != null)
            {
                plan.RootNode = ParsePlanNode(showPlanElement, 0);
            }

            plan.Initialize();
            return await Task.FromResult(plan).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing execution plan");
            return null;
        }
    }

    public async Task<List<string>> GetMissingIndexesAsync(QueryPlan plan)
    {
        _logger.LogInformation("Analyzing plan for missing indexes");

        var missingIndexes = new List<string>();

        // Find table scans
        var tableScans = plan.GetTableScans();
        foreach (var tableScan in tableScans)
        {
            if (!string.IsNullOrEmpty(tableScan.ObjectName))
            {
                missingIndexes.Add($"Consider adding index on {tableScan.ObjectName}");
            }
        }

        // Analyze table access patterns
        foreach (var access in plan.TableAccesses)
        {
            if (access.EstimatedRows > 1000 && access.AccessMethod == "Table Scan")
            {
                missingIndexes.Add(
                    $"High-volume table scan on {access.TableName} " +
                    $"({access.EstimatedRows:N0} rows estimated)");
            }
        }

        return await Task.FromResult(missingIndexes).ConfigureAwait(false);
    }

    public async Task<List<PerformanceIssue>> AnalyzePlanAsync(QueryPlan plan)
    {
        _logger.LogInformation("Analyzing execution plan for performance issues");

        var issues = new List<PerformanceIssue>();

        // Detect table scans
        var tableScans = plan.GetTableScans();
        foreach (var scan in tableScans)
        {
            if (scan.EstimatedRows > 1000)
            {
                issues.Add(new PerformanceIssue
                {
                    IssueType = IssueType.TableScan,
                    Severity = IssueSeverity.Warning,
                    Description = $"Table scan on {scan.ObjectName} with {scan.EstimatedRows:N0} estimated rows",
                    AffectedClause = "FROM",
                    EstimatedPerformanceImpact = 25.0,
                    RecommendedFix = "Add appropriate indexes for this table",
                    Priority = 1
                });
            }
        }

        // Detect inefficient joins
        foreach (var join in plan.Joins)
        {
            if (join.EstimatedCost > 5.0)
            {
                issues.Add(new PerformanceIssue
                {
                    IssueType = IssueType.IneffectiveJoin,
                    Severity = IssueSeverity.Warning,
                    Description = $"Expensive {join.JoinType} with estimated cost {join.EstimatedCost:F2}",
                    EstimatedPerformanceImpact = 20.0,
                    RecommendedFix = "Review join conditions and indexes",
                    Priority = 2
                });
            }
        }

        return await Task.FromResult(issues).ConfigureAwait(false);
    }

    private PlanNode ParsePlanNode(XElement element, int depth)
    {
        var node = new PlanNode
        {
            Depth = depth,
            NodeType = element.Name.LocalName,
            ObjectName = element.Attribute("Table")?.Value ?? string.Empty
        };

        // Extract metrics
        var physicalOpAttr = element.Attribute("PhysicalOp")?.Value;
        if (!string.IsNullOrEmpty(physicalOpAttr))
            node.NodeType = physicalOpAttr;

        var estimatedIOCost = double.TryParse(
            element.Attribute("EstimateIO")?.Value, out var io) ? io : 0;
        var estimatedCpuCost = double.TryParse(
            element.Attribute("EstimateCPU")?.Value, out var cpu) ? cpu : 0;

        node.EstimatedIoCost = estimatedIOCost;
        node.EstimatedCpuCost = estimatedCpuCost;
        node.EstimatedCost = estimatedIOCost + estimatedCpuCost;

        if (int.TryParse(element.Attribute("EstimateRows")?.Value, out var rows))
            node.EstimatedRows = rows;

        // Parse child nodes
        foreach (var child in element.Elements())
        {
            node.Children.Add(ParsePlanNode(child, depth + 1));
        }

        return node;
    }
}
