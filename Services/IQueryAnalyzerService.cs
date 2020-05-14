#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Repositories;
// Alias avoids CS0104 ambiguity with System.Index (range operator struct, .NET 5+).
using ModelIndex = SqlQueryAnalyzer.Models.Index;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Main service for analyzing SQL queries
/// </summary>
public interface IQueryAnalyzerService
{
    Task<QueryAnalysisResult> AnalyzeQueryAsync(string queryText);
    Task<QueryAnalysisResult> AnalyzeQueryAsync(DatabaseQuery query);

    // ValueTask avoids Task allocation on the synchronous (hot) path.
    ValueTask<double> CalculatePerformanceScoreAsync(QueryAnalysisResult analysis);
    ValueTask<QueryComplexity> DetermineComplexityAsync(DatabaseQuery query);
}

/// <summary>
/// Service for analyzing indexes
/// </summary>
public interface IIndexAnalyzerService
{
    Task<List<IndexSuggestion>> AnalyzeIndexesAsync(string tableName);
    Task<List<ModelIndex>> GetFragmentedIndexesAsync();
    Task<List<ModelIndex>> GetUnusedIndexesAsync();
    Task<IndexHealth> AssessIndexHealthAsync(ModelIndex index);
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
public class QueryAnalyzerService : IQueryAnalyzerService
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
            result.Issues = await _issueDetector.DetectIssuesAsync(query);

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
        var baseTime = result.Complexity switch
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
}
