// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Threading.Tasks;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Main service for analyzing SQL queries
/// </summary>
public interface IQueryAnalyzerService
{
    Task<QueryAnalysisResult> AnalyzeQueryAsync(string queryText);
    Task<QueryAnalysisResult> AnalyzeQueryAsync(DatabaseQuery query);
    Task<double> CalculatePerformanceScoreAsync(QueryAnalysisResult analysis);
    Task<QueryComplexity> DetermineComplexityAsync(DatabaseQuery query);
}

/// <summary>
/// Service for analyzing indexes
/// </summary>
public interface IIndexAnalyzerService
{
    Task<List<IndexSuggestion>> AnalyzeIndexesAsync(string tableName);
    Task<List<Index>> GetFragmentedIndexesAsync();
    Task<List<Index>> GetUnusedIndexesAsync();
    Task<IndexHealth> AssessIndexHealthAsync(Index index);
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
    Task<List<PerformanceIssue>> DetectNPlusOneAsync(List<DatabaseQuery> queries);
    Task<List<PerformanceIssue>> DetectJoinIssuesAsync(DatabaseQuery query);
    Task<List<PerformanceIssue>> DetectIndexOpportunitiesAsync(DatabaseQuery query);
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
            // Detect issues
            result.Issues = await _issueDetector.DetectIssuesAsync(query);

            // Get index suggestions
            if (query.ReferencedTables.Count > 0)
            {
                foreach (var table in query.ReferencedTables)
                {
                    var suggestions = await _indexAnalyzer.AnalyzeIndexesAsync(table);
                    result.IndexSuggestions.AddRange(suggestions);
                }
            }

            // Calculate performance score
            result.PerformanceScore = await CalculatePerformanceScoreAsync(result);

            // Estimate execution time
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

    public Task<double> CalculatePerformanceScoreAsync(QueryAnalysisResult analysis)
    {
        var score = 100.0;

        // Deduct for critical issues
        score -= analysis.Issues.Count(i => i.Severity == Constants.IssueSeverity.Critical) * 10;

        // Deduct for warnings
        score -= analysis.Issues.Count(i => i.Severity == Constants.IssueSeverity.Warning) * 5;

        // Deduct for info issues
        score -= analysis.Issues.Count(i => i.Severity == Constants.IssueSeverity.Info) * 2;

        // Bonus for optimization potential
        score += analysis.TotalOptimizationPotential * 0.1;

        return Task.FromResult(Math.Max(0, Math.Min(100, score)));
    }

    public Task<QueryComplexity> DetermineComplexityAsync(DatabaseQuery query)
    {
        var complexity = Constants.QueryComplexity.Simple;

        if (query.LineCount > 50)
            complexity = Constants.QueryComplexity.VeryHigh;
        else if (query.LineCount > 30)
            complexity = Constants.QueryComplexity.High;
        else if (query.LineCount > 15)
            complexity = Constants.QueryComplexity.Medium;
        else if (query.LineCount > 5)
            complexity = Constants.QueryComplexity.Low;

        if (query.ReferencedTables.Count > 5)
            complexity = Constants.QueryComplexity.High;

        if (query.JoinConditions.Count > 3)
            complexity = Constants.QueryComplexity.High;

        return Task.FromResult(complexity);
    }

    private TimeSpan EstimateExecutionTime(QueryAnalysisResult result)
    {
        var baseTime = result.Complexity switch
        {
            Constants.QueryComplexity.Simple => 10,
            Constants.QueryComplexity.Low => 50,
            Constants.QueryComplexity.Medium => 200,
            Constants.QueryComplexity.High => 500,
            Constants.QueryComplexity.VeryHigh => 1000,
            _ => 100
        };

        // Adjust for issues
        foreach (var issue in result.Issues)
        {
            baseTime += issue.EstimatedPerformanceImpact * 10;
        }

        return TimeSpan.FromMilliseconds(Math.Min(baseTime, 10000));
    }
}
