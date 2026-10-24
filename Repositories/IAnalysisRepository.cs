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

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryAnalysisRepository"/> class.
    /// </summary>
    public InMemoryAnalysisRepository(ILogger<InMemoryAnalysisRepository> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Save analysis asynchronously.
    /// </summary>
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

    /// <summary>
    /// Get analysis asynchronously.
    /// </summary>
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

    /// <summary>
    /// Get all analyses asynchronously.
    /// </summary>
    public Task<List<QueryAnalysisResult>> GetAllAnalysesAsync()
    {
        _logger.LogInformation("Retrieving all analyses");
        lock (_lock)
        {
            return Task.FromResult(_analyses.Values.ToList());
        }
    }

    /// <summary>
    /// Get analyses by date range asynchronously.
    /// </summary>
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

    /// <summary>
    /// Get analyses for query asynchronously.
    /// </summary>
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

    /// <summary>
    /// Delete analysis asynchronously.
    /// </summary>
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

    /// <summary>
    /// Get recent analyses asynchronously.
    /// </summary>
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

    /// <summary>
    /// Get issues by type asynchronously.
    /// </summary>
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

    /// <summary>
    /// Get critical issues asynchronously.
    /// </summary>
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

    /// <summary>
    /// Get total issue count asynchronously.
    /// </summary>
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
    /// <summary>
    /// Save index asynchronously.
    /// </summary>
    Task SaveIndexAsync(ModelIndex index);
    /// <summary>
    /// Get indexes for table asynchronously.
    /// </summary>
    Task<List<ModelIndex>> GetIndexesForTableAsync(string tableName);
    /// <summary>
    /// Get index asynchronously.
    /// </summary>
    Task<ModelIndex?> GetIndexAsync(string indexId);
    /// <summary>
    /// Delete index asynchronously.
    /// </summary>
    Task DeleteIndexAsync(string indexId);

    /// <summary>
    /// Get all indexes asynchronously.
    /// </summary>
    Task<List<ModelIndex>> GetAllIndexesAsync(); // Added missing method
    /// <summary>
    /// Get unused indexes asynchronously.
    /// </summary>
    Task<List<ModelIndex>> GetUnusedIndexesAsync(); // Added missing method
    /// <summary>
    /// Get fragmented indexes asynchronously.
    /// </summary>
    Task<List<ModelIndex>> GetFragmentedIndexesAsync(); // Added missing method
    /// <summary>
    /// Add index asynchronously.
    /// </summary>
    Task<ModelIndex> AddIndexAsync(ModelIndex index); // Changed return type
    /// <summary>
    /// Update index asynchronously.
    /// </summary>
    Task UpdateIndexAsync(ModelIndex index); // Changed parameter and return type
    /// <summary>
    /// Get suggestions asynchronously.
    /// </summary>
    Task<List<IndexSuggestion>> GetSuggestionsAsync(); // Added missing method
    /// <summary>
    /// Save suggestion asynchronously.
    /// </summary>
    Task SaveSuggestionAsync(IndexSuggestion suggestion); // Added missing method
    /// <summary>
    /// Get index count asynchronously.
    /// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryIndexRepository"/> class.
    /// </summary>
    public InMemoryIndexRepository(ILogger<InMemoryIndexRepository> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get index by name asynchronously.
    /// </summary>
    public Task<ModelIndex?> GetIndexByNameAsync(string indexName)
    {
        _logger.LogDebug("Retrieving index by name: {IndexName}", indexName);
        lock (_lock)
        {
            var index = _indexes.Values.FirstOrDefault(i => i.IndexName == indexName);
            return Task.FromResult(index);
        }
    }

    /// <summary>
    /// Get indexes by table asynchronously.
    /// </summary>
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

    /// <summary>
    /// Get all indexes asynchronously.
    /// </summary>
    public Task<List<ModelIndex>> GetAllIndexesAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_indexes.Values.ToList());
        }
    }

    /// <summary>
    /// Get unused indexes asynchronously.
    /// </summary>
    public Task<List<ModelIndex>> GetUnusedIndexesAsync()
    {
        lock (_lock)
        {
            var unused = _indexes.Values.Where(i => i.IsCandidateForRemoval).ToList();
            return Task.FromResult(unused);
        }
    }

    /// <summary>
    /// Get fragmented indexes asynchronously.
    /// </summary>
    public Task<List<ModelIndex>> GetFragmentedIndexesAsync()
    {
        lock (_lock)
        {
            var fragmented = _indexes.Values.Where(i => i.IsFragmented).ToList();
            return Task.FromResult(fragmented);
        }
    }

    /// <summary>
    /// Add index asynchronously.
    /// </summary>
    public Task<ModelIndex> AddIndexAsync(ModelIndex index)
    {
        _logger.LogInformation("Adding index: {IndexName} for table {TableName}", index.IndexName, index.TableName);
        lock (_lock)
        {
            _indexes[index.IndexId] = index;
            return Task.FromResult(index);
        }
    }

    /// <summary>
    /// Save index asynchronously.
    /// </summary>
    public async Task SaveIndexAsync(ModelIndex index)
    {
        await AddIndexAsync(index);
    }

    /// <summary>
    /// Get indexes for table asynchronously.
    /// </summary>
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

    /// <summary>
    /// Update index asynchronously.
    /// </summary>
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

    /// <summary>
    /// Get index asynchronously.
    /// </summary>
    public Task<ModelIndex?> GetIndexAsync(string indexId)
    {
        lock (_lock)
        {
            _indexes.TryGetValue(indexId, out var index);
            return Task.FromResult(index);
        }
    }

    /// <summary>
    /// Delete index asynchronously.
    /// </summary>
    public Task DeleteIndexAsync(string indexId)
    {
        _logger.LogInformation("Deleting index with ID: {IndexId}", indexId);
        lock (_lock)
        {
            _indexes.Remove(indexId);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Get suggestions asynchronously.
    /// </summary>
    public Task<List<IndexSuggestion>> GetSuggestionsAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(new List<IndexSuggestion>(_suggestions));
        }
    }

    /// <summary>
    /// Save suggestion asynchronously.
    /// </summary>
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

    /// <summary>
    /// Get index count asynchronously.
    /// </summary>
    public Task<int> GetIndexCountAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_indexes.Count);
        }
    }
}
