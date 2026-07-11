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
using SqlQueryAnalyzer.Services;
using SqlQueryAnalyzer.Models; // Already present, but confirming for clarity
using SqlQueryAnalyzer.Constants; // For IssueSeverity

namespace SqlQueryAnalyzer.Examples;

/// <summary>
/// Demonstrates execution plan analysis and bottleneck identification by parsing SQL Server execution plans,
/// analyzing performance issues, and identifying the most expensive operations in query execution.
/// </summary>
/// <remarks>
/// This example shows how to use the QueryPlanAnalyzerService to parse execution plan XML,
/// analyze it for performance issues, and identify bottlenecks that impact query performance.
/// </remarks>
public class ExecutionPlanAnalysisExample
{
    private readonly IQueryPlanAnalyzerService _planAnalyzer;
    private readonly ILogger<ExecutionPlanAnalysisExample> _logger;

    /// <summary>
/// Initializes a new instance of the ExecutionPlanAnalysisExample class.
/// </summary>
/// <param name="planAnalyzer">Service for analyzing SQL query execution plans.</param>
/// <param name="logger">Logger for recording execution and analysis process.</param>
public ExecutionPlanAnalysisExample(IQueryPlanAnalyzerService planAnalyzer, ILogger<ExecutionPlanAnalysisExample> logger)
    {
        _planAnalyzer = planAnalyzer;
        _logger = logger;
    }

    /// <summary>
/// Runs the execution plan analysis example demonstrating how to parse, analyze, and identify bottlenecks
/// in SQL Server execution plans.
/// </summary>
/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
public async Task RunExample()
    {
        _logger.LogInformation("Execution Plan Analysis Example");
        _logger.LogInformation("================================\n");

        // Example SQL Server execution plan XML
        var executionPlan = @"
            <ShowPlanXML>
              <BatchSequence>
                <Batch>
                  <Statements>
                    <StmtSimple StatementCompId='1' StatementEstRows='100'
                                EstimatedTotalSubtreeCost='0.0234' StatementType='SELECT'>
                      <StatementText>SELECT * FROM Orders WHERE CustomerId = @CustomerID</StatementText>
                      <RelOp NodeId='0' Parent='0' PhysicalOp='Index Seek'
                              EstimatedRows='100' EstimatedIO='0.01' EstimatedCPU='0.002'
                              EstimatedTotalSubtreeCost='0.0234'>
                        <OutputList>
                          <ColumnReference Column='Id' />
                          <ColumnReference Column='CustomerId' />
                          <ColumnReference Column='OrderDate' />
                        </OutputList>
                      </RelOp>
                    </StmtSimple>
                  </Statements>
                </Batch>
              </BatchSequence>
            </ShowPlanXML>
        ";

        _logger.LogInformation("Parsing execution plan...\n");

        try
        {
            var plan = await _planAnalyzer.ParseSqlServerPlanAsync(executionPlan);

            if (plan == null)
            {
                _logger.LogError("Failed to parse execution plan.");
                return;
            }

            DisplayPlanSummary(plan, _logger);

            _logger.LogInformation("\n---\n");

            var issues = await _planAnalyzer.AnalyzePlanAsync(plan);
            DisplayPlanIssues(issues, _logger);

            _logger.LogInformation("\n---\n");

            // Assuming GetExpensiveOperations is the closest to what FindBottlenecksAsync was meant to do
            var bottlenecks = plan.GetExpensiveOperations(3);
            DisplayBottlenecks(bottlenecks, _logger);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Plan analysis failed");
        }
    }

    /// <summary>
/// Displays a summary of the execution plan including total cost, estimated rows, and resource usage.
/// </summary>
/// <param name="plan">The parsed query execution plan to display.</param>
/// <param name="logger">Logger for outputting the summary information.</param>
static void DisplayPlanSummary(QueryPlan plan, ILogger logger)
    {
        logger.LogInformation("EXECUTION PLAN SUMMARY");
        logger.LogInformation("=====================\n");

        logger.LogInformation($"Total Cost: {plan.TotalEstimatedCost:F4}");
        logger.LogInformation($"Estimated Rows: {plan.TotalEstimatedRows:N0}");
        logger.LogInformation($"Estimated I/O: {plan.TotalEstimatedIoCost:F4}");
        logger.LogInformation($"Estimated CPU: {plan.TotalEstimatedCpuCost:F6}");
        logger.LogInformation($"Root Operation: {plan.RootNode?.NodeType}");
        logger.LogInformation($"Plan Format: {plan.Format}");
    }

    /// <summary>
/// Displays a cost breakdown analysis showing the most expensive operations in the execution plan.
/// </summary>
/// <param name="operations">List of plan nodes to analyze and display.</param>
/// <param name="logger">Logger for outputting the cost breakdown information.</param>
static void DisplayCostAnalysis(List<PlanNode> operations, ILogger logger)
    {
        logger.LogInformation("COST BREAKDOWN BY OPERATION");
        logger.LogInformation("===========================\n");

        var totalCost = operations.Sum(o => o.EstimatedCost);

        foreach (var op in operations.Take(10))
        {
            var percentage = (op.EstimatedCost / totalCost) * 100;
            var bar = new string('█', (int)(percentage / 5));

            logger.LogInformation($"{op.NodeType,-25} {op.EstimatedCost:F4}  ({percentage:F1}%) {bar}");
            logger.LogInformation($"  Estimated Rows: {op.EstimatedRows:N0}, I/O: {op.EstimatedIoCost:F4}");
        }
    }

    /// <summary>
/// Displays a list of performance issues found during execution plan analysis, grouped by severity.
/// </summary>
/// <param name="issues">List of performance issues detected in the execution plan.</param>
/// <param name="logger">Logger for outputting the issues and recommendations.</param>
static void DisplayPlanIssues(List<PerformanceIssue> issues, ILogger logger)
    {
        logger.LogInformation("PLAN ANALYSIS ISSUES");
        logger.LogInformation("====================\n");

        if (issues.Count == 0)
        {
            logger.LogInformation("✓ No significant issues detected in the execution plan");
            return;
        }

        logger.LogWarning($"Found {issues.Count} issue(s):\n");

        var bySeverity = issues.GroupBy(i => i.Severity).OrderByDescending(g => g.Key);

        foreach (var group in bySeverity)
        {
            logger.LogWarning($"\n{group.Key}:");
            foreach (var issue in group)
            {
                logger.LogWarning($"  • {issue.IssueType}");
                logger.LogWarning($"    Description: {issue.Description}");
                logger.LogWarning($"    Recommendation: {issue.RecommendedFix}");
            }
        }
    }

    /// <summary>
/// Displays the most expensive operations identified as performance bottlenecks in the execution plan.
/// </summary>
/// <param name="bottlenecks">List of plan nodes identified as performance bottlenecks, ordered by cost.</param>
/// <param name="logger">Logger for outputting the bottleneck information and recommendations.</param>
static void DisplayBottlenecks(List<PlanNode> bottlenecks, ILogger logger)
    {
        logger.LogInformation("PERFORMANCE BOTTLENECKS");
        logger.LogInformation("=======================\n");

        if (bottlenecks.Count == 0)
        {
            logger.LogInformation("✓ No significant bottlenecks identified");
            return;
        }

        foreach (var bottleneck in bottlenecks.OrderByDescending(b => b.EstimatedCost))
        {
            logger.LogError($"🔴 {bottleneck.NodeType}");
            logger.LogError($"   Impact: {bottleneck.EstimatedCost:F2}");
            logger.LogError($"   Description: {bottleneck.NodeType} operation on {bottleneck.ObjectName}");
            logger.LogError($"   Recommendation: Consider optimizing this operation.");
            logger.LogError();
        }
    }
}
