#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Models;
using ModelIndex = SqlQueryAnalyzer.Models.Index;

namespace SqlQueryAnalyzer.Repositories;

/// <summary>
/// Repository interface for persisting analysis results.
/// Abstraction for storage layer (database, file system, cloud).
/// Enables testing with mock implementations.
/// </summary>
public interface IAnalysisRepository
{
    /// <summary>
    /// Saves an analysis result.
    /// </summary>
    Task<QueryAnalysisResult> SaveAnalysisAsync(QueryAnalysisResult result);

    /// <summary>
    /// Retrieves analysis by ID.
    /// </summary>
    Task<QueryAnalysisResult?> GetAnalysisAsync(string analysisId);

    /// <summary>
    /// Gets all analyses for a specific query.
    /// </summary>
    Task<List<QueryAnalysisResult>> GetAnalysesForQueryAsync(string queryHash);

    /// <summary>
    /// Deletes an analysis.
    /// </summary>
    Task DeleteAnalysisAsync(string analysisId);

    /// <summary>
    /// Gets recent analyses.
    /// </summary>
    Task<List<QueryAnalysisResult>> GetRecentAnalysesAsync(int count = 100);
}

/// <summary>
/// In-memory implementation of analysis repository.
/// Used for development and testing.
/// </summary>
public class InMemoryAnalysisRepository : IAnalysisRepository
{
    private readonly Dictionary<string, QueryAnalysisResult> _analyses = new();
    private readonly object _lock = new object();
    private readonly ILogger<InMemoryAnalysisRepository> _logger;

    public InMemoryAnalysisRepository(ILogger<InMemoryAnalysisRepository> logger)
    {
        _logger = logger;
    }

    public Task<QueryAnalysisResult> SaveAnalysisAsync(QueryAnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        _logger.LogInformation("Saving analysis for query: {QueryId}", result.QueryId);
        lock (_lock)
        {
            _analyses[result.QueryId] = result;
        }

        return Task.FromResult(result);
    }

    public Task<QueryAnalysisResult?> GetAnalysisAsync(string analysisId)
    {
        ArgumentException.ThrowIfNullOrEmpty(analysisId);
        _logger.LogDebug("Retrieving analysis: {AnalysisId}", analysisId);
        lock (_lock)
        {
            _analyses.TryGetValue(analysisId, out var result);
            return Task.FromResult(result);
        }
    }

    public Task<List<QueryAnalysisResult>> GetAllAnalysesAsync()
    {
        _logger.LogInformation("Retrieving all analyses");
        lock (_lock)
        {
            return Task.FromResult(_analyses.Values.ToList());
        }
    }

    public Task<List<QueryAnalysisResult>> GetAnalysesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        lock (_lock)
        {
            var results = _analyses.Values
                .Where(a => a.AnalyzedAt >= startDate && a.AnalyzedAt <= endDate)
                .ToList();
            return Task.FromResult(results);
        }
    }

    public Task<List<QueryAnalysisResult>> GetAnalysesForQueryAsync(string queryHash)
    {
        ArgumentException.ThrowIfNullOrEmpty(queryHash);
        lock (_lock)
        {
            var results = _analyses.Values
                .Where(a => a.Query.GetHashCode().ToString() == queryHash)
                .ToList();

            return Task.FromResult(results);
        }
    }

    public Task DeleteAnalysisAsync(string analysisId)
    {
        ArgumentException.ThrowIfNullOrEmpty(analysisId);
        _logger.LogInformation("Deleting analysis: {AnalysisId}", analysisId);
        lock (_lock)
        {
            _analyses.Remove(analysisId);
        }

        return Task.CompletedTask;
    }

    public Task<List<QueryAnalysisResult>> GetRecentAnalysesAsync(int count = 100)
    {
        lock (_lock)
        {
            var results = _analyses.Values
                .OrderByDescending(a => a.AnalyzedAt)
                .Take(count)
                .ToList();

            return Task.FromResult(results);
        }
    }

    public Task<List<PerformanceIssue>> GetIssuesByTypeAsync(Constants.IssueType issueType)
    {
        lock (_lock)
        {
            var issues = _analyses.Values
                .SelectMany(a => a.Issues)
                .Where(i => i.IssueType == issueType)
                .ToList();
            return Task.FromResult(issues);
        }
    }

    public Task<List<PerformanceIssue>> GetCriticalIssuesAsync()
    {
        lock (_lock)
        {
            var issues = _analyses.Values
                .SelectMany(a => a.Issues)
                .Where(i => i.Severity == Constants.IssueSeverity.Critical)
                .ToList();
            return Task.FromResult(issues);
        }
    }

    public Task<int> GetTotalIssueCountAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_analyses.Values.Sum(a => a.Issues.Count));
        }
    }
}

/// <summary>
/// Repository interface for storing indexes.
/// </summary>
public interface IIndexRepository
{
    Task SaveIndexAsync(ModelIndex index);
    Task<List<ModelIndex>> GetIndexesForTableAsync(string tableName);
    Task<ModelIndex?> GetIndexAsync(string indexId);
    Task DeleteIndexAsync(string indexId);

    Task<List<ModelIndex>> GetAllIndexesAsync(); // Added missing method
    Task<List<ModelIndex>> GetUnusedIndexesAsync(); // Added missing method
    Task<List<ModelIndex>> GetFragmentedIndexesAsync(); // Added missing method
    Task<ModelIndex> AddIndexAsync(ModelIndex index); // Changed return type
    Task UpdateIndexAsync(ModelIndex index); // Changed parameter and return type
    Task<List<IndexSuggestion>> GetSuggestionsAsync(); // Added missing method
    Task SaveSuggestionAsync(IndexSuggestion suggestion); // Added missing method
    Task<int> GetIndexCountAsync(); // Added missing method
}

/// <summary>
/// In-memory implementation of index repository.
/// </summary>
public class InMemoryIndexRepository : IIndexRepository
{
    private readonly Dictionary<string, ModelIndex> _indexes = new();
    private readonly List<IndexSuggestion> _suggestions = new();
    private readonly object _lock = new object();
    private readonly ILogger<InMemoryIndexRepository> _logger;

    public InMemoryIndexRepository(ILogger<InMemoryIndexRepository> logger)
    {
        _logger = logger;
    }

    public Task<ModelIndex?> GetIndexByNameAsync(string indexName)
    {
        _logger.LogDebug("Retrieving index by name: {IndexName}", indexName);
        lock (_lock)
        {
            var index = _indexes.Values.FirstOrDefault(i => i.IndexName == indexName);
            return Task.FromResult(index);
        }
    }

    public Task<List<ModelIndex>> GetIndexesByTableAsync(string tableName)
    {
        lock (_lock)
        {
            var results = _indexes.Values
                .Where(i => i.TableName == tableName)
                .ToList();

            return Task.FromResult(results);
        }
    }

    public Task<List<ModelIndex>> GetAllIndexesAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_indexes.Values.ToList());
        }
    }

    public Task<List<ModelIndex>> GetUnusedIndexesAsync()
    {
        lock (_lock)
        {
            var unused = _indexes.Values.Where(i => i.IsCandidateForRemoval).ToList();
            return Task.FromResult(unused);
        }
    }

    public Task<List<ModelIndex>> GetFragmentedIndexesAsync()
    {
        lock (_lock)
        {
            var fragmented = _indexes.Values.Where(i => i.IsFragmented).ToList();
            return Task.FromResult(fragmented);
        }
    }

    public Task<ModelIndex> AddIndexAsync(ModelIndex index)
    {
        _logger.LogInformation("Adding index: {IndexName} for table {TableName}", index.IndexName, index.TableName);
        lock (_lock)
        {
            _indexes[index.IndexId] = index;
            return Task.FromResult(index);
        }
    }

    public async Task SaveIndexAsync(ModelIndex index)
    {
        await AddIndexAsync(index);
    }

    public Task<List<ModelIndex>> GetIndexesForTableAsync(string tableName)
    {
        lock (_lock)
        {
            var results = _indexes.Values
                .Where(i => string.Equals(i.TableName, tableName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Task.FromResult(results);
        }
    }

    public Task UpdateIndexAsync(ModelIndex index)
    {
        _logger.LogInformation("Updating index: {IndexName}", index.IndexName);
        lock (_lock)
        {
            if (_indexes.ContainsKey(index.IndexId))
            {
                _indexes[index.IndexId] = index;
            }
            return Task.CompletedTask;
        }
    }

    public Task<ModelIndex?> GetIndexAsync(string indexId)
    {
        lock (_lock)
        {
            _indexes.TryGetValue(indexId, out var index);
            return Task.FromResult(index);
        }
    }

    public Task DeleteIndexAsync(string indexId)
    {
        _logger.LogInformation("Deleting index with ID: {IndexId}", indexId);
        lock (_lock)
        {
            _indexes.Remove(indexId);
        }

        return Task.CompletedTask;
    }

    public Task<List<IndexSuggestion>> GetSuggestionsAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(new List<IndexSuggestion>(_suggestions));
        }
    }

    public Task SaveSuggestionAsync(IndexSuggestion suggestion)
    {
        lock (_lock)
        {
            var existing = _suggestions.FirstOrDefault(s => s.SuggestionId == suggestion.SuggestionId);
            if (existing != null)
                _suggestions.Remove(existing);
            suggestion.GenerateIndexName();
            suggestion.GenerateCreateScript();
            suggestion.GenerateDropScript();
            _suggestions.Add(suggestion);
            return Task.CompletedTask;
        }
    }

    public Task<int> GetIndexCountAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_indexes.Count);
        }
    }
}
