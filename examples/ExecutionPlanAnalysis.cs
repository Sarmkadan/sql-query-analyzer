// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Services;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Examples;

/// Demonstrates execution plan analysis and bottleneck identification
class ExecutionPlanAnalysis
{
    static async Task Main()
    {
        var services = new ServiceCollection()
            .AddLogging(config => config.AddConsole())
            .AddScoped<IQueryPlanAnalyzerService, QueryPlanAnalyzerService>()
            .BuildServiceProvider();

        var planAnalyzer = services.GetRequiredService<IQueryPlanAnalyzerService>();
        var logger = services.GetRequiredService<ILogger<ExecutionPlanAnalysis>>();

        logger.LogInformation("Execution Plan Analysis Example");
        logger.LogInformation("================================\n");

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

        logger.LogInformation("Parsing execution plan...\n");

        try
        {
            var plan = await planAnalyzer.ParsePlanAsync(executionPlan);

            DisplayPlanSummary(plan, logger);

            logger.LogInformation("\n---\n");

            var operations = await planAnalyzer.GetOperationsByTotalCostAsync(plan);
            DisplayCostAnalysis(operations, logger);

            logger.LogInformation("\n---\n");

            var issues = await planAnalyzer.AnalyzePlanAsync(plan);
            DisplayPlanIssues(issues, logger);

            logger.LogInformation("\n---\n");

            var bottlenecks = await planAnalyzer.FindBottlenecksAsync(plan);
            DisplayBottlenecks(bottlenecks, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Plan analysis failed");
        }
    }

    static void DisplayPlanSummary(QueryPlan plan, ILogger logger)
    {
        logger.LogInformation("EXECUTION PLAN SUMMARY");
        logger.LogInformation("=====================\n");

        logger.LogInformation($"Total Cost: {plan.TotalCost:F4}");
        logger.LogInformation($"Estimated Rows: {plan.EstimatedRows:N0}");
        logger.LogInformation($"Estimated I/O: {plan.EstimatedIO:F4}");
        logger.LogInformation($"Estimated CPU: {plan.EstimatedCPU:F6}");
        logger.LogInformation($"Root Operation: {plan.RootOperation?.OperationType}");
        logger.LogInformation($"Plan Source: {plan.PlanSource}");
    }

    static void DisplayCostAnalysis(List<PlanOperation> operations, ILogger logger)
    {
        logger.LogInformation("COST BREAKDOWN BY OPERATION");
        logger.LogInformation("===========================\n");

        var totalCost = operations.Sum(o => o.TotalCost);

        foreach (var op in operations.Take(10))
        {
            var percentage = (op.TotalCost / totalCost) * 100;
            var bar = new string('█', (int)(percentage / 5));

            logger.LogInformation($"{op.OperationType,-25} {op.TotalCost:F4}  ({percentage:F1}%) {bar}");
            logger.LogInformation($"  Estimated Rows: {op.EstimatedRows:N0}, I/O: {op.EstimatedIO:F4}");
        }
    }

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

    static void DisplayBottlenecks(List<PlanBottleneck> bottlenecks, ILogger logger)
    {
        logger.LogInformation("PERFORMANCE BOTTLENECKS");
        logger.LogInformation("=======================\n");

        if (bottlenecks.Count == 0)
        {
            logger.LogInformation("✓ No significant bottlenecks identified");
            return;
        }

        foreach (var bottleneck in bottlenecks.OrderByDescending(b => b.Impact))
        {
            logger.LogError($"🔴 {bottleneck.Name}");
            logger.LogError($"   Impact: {bottleneck.Impact}/10");
            logger.LogError($"   Description: {bottleneck.Description}");
            logger.LogError($"   Recommendation: {bottleneck.Recommendation}");
            logger.LogError();
        }
    }
}
