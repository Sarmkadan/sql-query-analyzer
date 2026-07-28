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
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Repositories;
// Alias avoids CS0104 ambiguity with System.Index (range operator struct, .NET 5+).
using ModelIndex = SqlQueryAnalyzer.Models.Index;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Provides methods for analyzing SQL queries to detect performance issues and optimize them.
/// </summary>
public interface IQueryAnalyzerService : IDisposable
{
    /// <summary>
    /// Analyzes a raw SQL query string for performance issues.
    /// </summary>
    /// <param name="queryText">The SQL query string.</param>
    /// <returns>A <see cref="QueryAnalysisResult"/> containing the analysis findings.</returns>
    Task<QueryAnalysisResult> AnalyzeQueryAsync(string queryText);

    /// <summary>
    /// Analyzes a <see cref="DatabaseQuery"/> object for performance issues.
    /// </summary>
    /// <param name="query">The <see cref="DatabaseQuery"/> to analyze.</param>
    /// <returns>A <see cref="QueryAnalysisResult"/> containing the analysis findings.</returns>
    Task<QueryAnalysisResult> AnalyzeQueryAsync(DatabaseQuery query);

    /// <summary>
    /// Calculates the performance score of a given analysis result.
    /// </summary>
    /// <param name="analysis">The analysis result to score.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the performance score (0-100).</returns>
    ValueTask<double> CalculatePerformanceScoreAsync(QueryAnalysisResult analysis);

    /// <summary>
    /// Determines the complexity level of a given query.
    /// </summary>
    /// <param name="query">The query to assess.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the query complexity.</returns>
    ValueTask<QueryComplexity> DetermineComplexityAsync(DatabaseQuery query);
}

/// <summary>
/// Provides methods for analyzing database indexes to identify optimization opportunities and maintenance needs.
/// </summary>
public interface IIndexAnalyzerService
{
    /// <summary>
    /// Analyzes indexes for a given table.
    /// </summary>
    /// <param name="tableName">The name of the table to analyze.</param>
    /// <returns>A list of <see cref="IndexSuggestion"/> for the table.</returns>
    Task<List<IndexSuggestion>> AnalyzeIndexesAsync(string tableName);

    /// <summary>
    /// Retrieves a list of fragmented indexes.
    /// </summary>
    /// <returns>A list of fragmented <see cref="ModelIndex"/> objects.</returns>
    Task<List<ModelIndex>> GetFragmentedIndexesAsync();

    /// <summary>
    /// Retrieves a list of unused indexes.
    /// </summary>
    /// <returns>A list of unused <see cref="ModelIndex"/> objects.</returns>
    Task<List<ModelIndex>> GetUnusedIndexesAsync();

    /// <summary>
    /// Assesses the health of a given index.
    /// </summary>
    /// <param name="index">The index to assess.</param>
    /// <returns>The <see cref="IndexHealth"/> status of the index.</returns>
    Task<IndexHealth> AssessIndexHealthAsync(ModelIndex index);

    /// <summary>
    /// Generates maintenance scripts for all indexes.
    /// </summary>
    /// <returns>A list of SQL maintenance scripts.</returns>
    Task<List<string>> GenerateMaintenanceScriptsAsync();
}

/// <summary>
/// Service for analyzing query execution plans
/// </summary>
public interface IQueryPlanAnalyzerService
{
    Task<QueryPlan?> ParseExecutionPlanAsync(string planXml);
    Task<List<string>> GetMissingIndexesAsync(QueryPlan plan);
    Task<List<PerformanceIssue>> AnalyzePlanAsync(QueryPlan plan);
}

/// <summary>
/// Service for detecting performance issues
/// </summary>
public interface IPerformanceIssueDetectorService
{
    Task<List<PerformanceIssue>> DetectIssuesAsync(DatabaseQuery query);

    /// <summary>
    /// Runs every registered rule plugin against the query, isolating each plugin behind its
    /// own timeout and exception boundary, and returns both the successfully detected issues
    /// and a diagnostic entry for every plugin that failed or timed out.
    /// </summary>
    /// <param name="query">The query to analyze.</param>
    /// <param name="cancellationToken">Token used to cancel the whole detection run.</param>
    /// <returns>The detected issues alongside diagnostics for any plugin that could not complete.</returns>
    Task<(List<PerformanceIssue> Issues, List<Models.AnalysisDiagnostic> Diagnostics)> DetectIssuesWithDiagnosticsAsync(
        DatabaseQuery query,
        CancellationToken cancellationToken = default);

    // These methods complete synchronously; ValueTask avoids the Task allocation.
    ValueTask<List<PerformanceIssue>> DetectNPlusOneAsync(List<DatabaseQuery> queries);
    ValueTask<List<PerformanceIssue>> DetectJoinIssuesAsync(DatabaseQuery query);
    ValueTask<List<PerformanceIssue>> DetectIndexOpportunitiesAsync(DatabaseQuery query);
}

/// <summary>
/// Service for parsing EXPLAIN PLAN output
/// </summary>
public interface IExplainPlanParserService
{
    Task<QueryPlan> ParseSqlServerPlanAsync(string xmlPlan);
    Task<QueryPlan> ParsePostgreSqlPlanAsync(string jsonPlan);
    Task<QueryPlan> ParseMySqlPlanAsync(string jsonPlan);
    Task<Dictionary<string, object>> ExtractPlanMetricsAsync(QueryPlan plan);
}

/// <summary>
/// Implementation of query analyzer service
/// </summary>
public class QueryAnalyzerService : IQueryAnalyzerService, IDisposable
{
    private readonly IPerformanceIssueDetectorService _issueDetector;
    private readonly IIndexAnalyzerService _indexAnalyzer;
    private readonly IAnalysisRepository _repository;
    private readonly Microsoft.Extensions.Logging.ILogger<QueryAnalyzerService> _logger;

    public QueryAnalyzerService(
        IPerformanceIssueDetectorService issueDetector,
        IIndexAnalyzerService indexAnalyzer,
        IAnalysisRepository repository,
        Microsoft.Extensions.Logging.ILogger<QueryAnalyzerService> logger)
    {
        _issueDetector = issueDetector;
        _indexAnalyzer = indexAnalyzer;
        _repository = repository;
        _logger = logger;
    }

    public async Task<QueryAnalysisResult> AnalyzeQueryAsync(string queryText)
    {
        var query = new DatabaseQuery { QueryText = queryText };
        query.Parse();
        return await AnalyzeQueryAsync(query);
    }

    public async Task<QueryAnalysisResult> AnalyzeQueryAsync(DatabaseQuery query)
    {
        _logger.LogInformation($"Analyzing query: {query.QueryId}");

        var result = new QueryAnalysisResult
        {
            Query = query.QueryText,
            Complexity = await DetermineComplexityAsync(query),
            Statistics = new QueryStatistics
            {
                ExecutionCount = 1,
                FirstExecution = DateTime.UtcNow,
                LastExecution = DateTime.UtcNow
            }
        };

        try
        {
            var (issues, diagnostics) = await _issueDetector.DetectIssuesWithDiagnosticsAsync(query);
            result.Issues = issues;

            if (diagnostics.Count > 0)
            {
                result.Diagnostics.AddRange(diagnostics);
                result.IsPartial = true;
                _logger.LogWarning($"{diagnostics.Count} detector(s) failed or timed out - analysis is partial");
            }

            if (query.ReferencedTables.Count > 0)
            {
                foreach (var table in query.ReferencedTables)
                {
                    var suggestions = await _indexAnalyzer.AnalyzeIndexesAsync(table);
                    result.IndexSuggestions.AddRange(suggestions);
                }
            }

            result.PerformanceScore = await CalculatePerformanceScoreAsync(result);
            result.EstimatedExecutionTime = EstimateExecutionTime(result);

            await _repository.SaveAnalysisAsync(result);
            _logger.LogInformation($"Analysis completed. Score: {result.PerformanceScore:F1}/100");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing query");
            throw;
        }

        return result;
    }

    // ValueTask.FromResult avoids a heap allocation when the result is already known.
    public ValueTask<double> CalculatePerformanceScoreAsync(QueryAnalysisResult analysis)
    {
        var score = 100.0;

        score -= analysis.Issues.Count(i => i.Severity == IssueSeverity.Critical) * 10;
        score -= analysis.Issues.Count(i => i.Severity == IssueSeverity.Warning) * 5;
        score -= analysis.Issues.Count(i => i.Severity == IssueSeverity.Info) * 2;

        score += analysis.TotalOptimizationPotential * 0.1;

        return ValueTask.FromResult(Math.Max(0, Math.Min(100, score)));
    }

    public ValueTask<QueryComplexity> DetermineComplexityAsync(DatabaseQuery query)
    {
        var complexity = QueryComplexity.Simple;

        if (query.LineCount > 50)
            complexity = QueryComplexity.VeryHigh;
        else if (query.LineCount > 30)
            complexity = QueryComplexity.High;
        else if (query.LineCount > 15)
            complexity = QueryComplexity.Medium;
        else if (query.LineCount > 5)
            complexity = QueryComplexity.Low;

        if (query.ReferencedTables.Count > 5)
            complexity = QueryComplexity.High;

        if (query.JoinConditions.Count > 3)
            complexity = QueryComplexity.High;

        return ValueTask.FromResult(complexity);
    }

    private TimeSpan EstimateExecutionTime(QueryAnalysisResult result)
    {
        double baseTime = result.Complexity switch
        {
            QueryComplexity.Simple => 10,
            QueryComplexity.Low => 50,
            QueryComplexity.Medium => 200,
            QueryComplexity.High => 500,
            QueryComplexity.VeryHigh => 1000,
            _ => 100
        };

        foreach (var issue in result.Issues)
            baseTime += issue.EstimatedPerformanceImpact * 10;

        return TimeSpan.FromMilliseconds(Math.Min(baseTime, 10000));
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // Currently, no managed resources to dispose.
        // If any dependencies implement IDisposable, they should be disposed here.
    }
}
