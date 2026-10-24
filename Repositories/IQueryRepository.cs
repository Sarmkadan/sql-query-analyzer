#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Models;
using ModelIndex = SqlQueryAnalyzer.Models.Index;
using SqlQueryAnalyzer.Constants;

namespace SqlQueryAnalyzer.Repositories;

/// <summary>
/// Repository interface for query operations
/// </summary>
public interface IQueryRepository
{
    /// <summary>
    /// Get query by ID asynchronously.
    /// </summary>
    Task<DatabaseQuery?> GetQueryByIdAsync(string queryId);
    /// <summary>
    /// Get all queries asynchronously.
    /// </summary>
    Task<List<DatabaseQuery>> GetAllQueriesAsync();
    /// <summary>
    /// Get queries by table asynchronously.
    /// </summary>
    Task<List<DatabaseQuery>> GetQueriesByTableAsync(string tableName);
    /// <summary>
    /// Get queries by type asynchronously.
    /// </summary>
    Task<List<DatabaseQuery>> GetQueriesByTypeAsync(QueryType queryType);
    /// <summary>
    /// Add query asynchronously.
    /// </summary>
    Task<DatabaseQuery> AddQueryAsync(DatabaseQuery query);
    /// <summary>
    /// Update query asynchronously.
    /// </summary>
    Task UpdateQueryAsync(DatabaseQuery query);
    /// <summary>
    /// Delete query asynchronously.
    /// </summary>
    Task DeleteQueryAsync(string queryId);
    /// <summary>
    /// Search queries asynchronously.
    /// </summary>
    Task<List<DatabaseQuery>> SearchQueriesAsync(string searchTerm);
    /// <summary>
    /// Get queries by application asynchronously.
    /// </summary>
    Task<List<DatabaseQuery>> GetQueriesByApplicationAsync(string applicationName);
    /// <summary>
    /// Get query count asynchronously.
    /// </summary>
    Task<int> GetQueryCountAsync();
}



/// <summary>
/// In-memory implementation of query repository for Phase 1
/// </summary>
public class QueryRepository : IQueryRepository
{
    private readonly List<DatabaseQuery> _queries = [];
    private readonly object _lock = new();
    private readonly ILogger<QueryRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryRepository"/> class.
    /// </summary>
    public QueryRepository(ILogger<QueryRepository> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get query by ID asynchronously.
    /// </summary>
    public Task<DatabaseQuery?> GetQueryByIdAsync(string queryId)
    {
        ArgumentException.ThrowIfNullOrEmpty(queryId);
        _logger.LogDebug("Retrieving query by ID: {QueryId}", queryId);
        lock (_lock)
        {
            var query = _queries.FirstOrDefault(q => q.QueryId == queryId);
            return Task.FromResult(query);
        }
    }

    /// <summary>
    /// Get all queries asynchronously.
    /// </summary>
    public Task<List<DatabaseQuery>> GetAllQueriesAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(new List<DatabaseQuery>(_queries));
        }
    }

    /// <summary>
    /// Get queries by table asynchronously.
    /// </summary>
    public Task<List<DatabaseQuery>> GetQueriesByTableAsync(string tableName)
    {
        ArgumentException.ThrowIfNullOrEmpty(tableName);
        lock (_lock)
        {
            var queries = _queries
                .Where(q => q.ReferencedTables.Contains(tableName, StringComparer.OrdinalIgnoreCase))
                .ToList();
            return Task.FromResult(queries);
        }
    }

    /// <summary>
    /// Get queries by type asynchronously.
    /// </summary>
    public Task<List<DatabaseQuery>> GetQueriesByTypeAsync(QueryType queryType)
    {
        lock (_lock)
        {
            var queries = _queries.Where(q => q.QueryType == queryType).ToList();
            return Task.FromResult(queries);
        }
    }

    /// <summary>
    /// Add query asynchronously.
    /// </summary>
    public Task<DatabaseQuery> AddQueryAsync(DatabaseQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        _logger.LogInformation("Adding new query: {QueryId}", query.QueryId);
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(query.QueryHash))
                query.GenerateHash();
            _queries.Add(query);
            return Task.FromResult(query);
        }
    }

    /// <summary>
    /// Update query asynchronously.
    /// </summary>
    public Task UpdateQueryAsync(DatabaseQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        _logger.LogInformation("Updating query: {QueryId}", query.QueryId);
        lock (_lock)
        {
            var existing = _queries.FirstOrDefault(q => q.QueryId == query.QueryId);
            if (existing != null)
            {
                _queries.Remove(existing);
                _queries.Add(query);
            }
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Delete query asynchronously.
    /// </summary>
    public Task DeleteQueryAsync(string queryId)
    {
        ArgumentException.ThrowIfNullOrEmpty(queryId);
        _logger.LogInformation("Deleting query: {QueryId}", queryId);
        lock (_lock)
        {
            _queries.RemoveAll(q => q.QueryId == queryId);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Search queries asynchronously.
    /// </summary>
    public Task<List<DatabaseQuery>> SearchQueriesAsync(string searchTerm)
    {
        ArgumentException.ThrowIfNullOrEmpty(searchTerm);
        lock (_lock)
        {
            var results = _queries
                .Where(q => q.QueryText.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           q.ProcedureName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
            return Task.FromResult(results);
        }
    }

    /// <summary>
    /// Get queries by application asynchronously.
    /// </summary>
    public Task<List<DatabaseQuery>> GetQueriesByApplicationAsync(string applicationName)
    {
        ArgumentException.ThrowIfNullOrEmpty(applicationName);
        lock (_lock)
        {
            var queries = _queries
                .Where(q => q.ApplicationName == applicationName)
                .ToList();
            return Task.FromResult(queries);
        }
    }

    /// <summary>
    /// Get query count asynchronously.
    /// </summary>
    public Task<int> GetQueryCountAsync()
    {
        _logger.LogDebug("Getting query count: {Count}", _queries.Count);
        lock (_lock)
        {
            return Task.FromResult(_queries.Count);
        }
    }
}

/// <summary>
/// In-memory implementation of analysis repository
/// </summary>
public class AnalysisRepository : IAnalysisRepository
{
    private readonly List<QueryAnalysisResult> _analyses = [];
    private readonly object _lock = new();

    /// <summary>
    /// Get analysis asynchronously.
    /// </summary>
    public Task<QueryAnalysisResult?> GetAnalysisAsync(string queryId)
    {
        lock (_lock)
        {
            var analysis = _analyses.FirstOrDefault(a => a.QueryId == queryId);
            return Task.FromResult(analysis);
        }
    }

    /// <summary>
    /// Get all analyses asynchronously.
    /// </summary>
    public Task<List<QueryAnalysisResult>> GetAllAnalysesAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(new List<QueryAnalysisResult>(_analyses));
        }
    }

    /// <summary>
    /// Get analyses by date range asynchronously.
    /// </summary>
    public Task<List<QueryAnalysisResult>> GetAnalysesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        lock (_lock)
        {
            var results = _analyses
                .Where(a => a.AnalyzedAt >= startDate && a.AnalyzedAt <= endDate)
                .ToList();
            return Task.FromResult(results);
        }
    }

    /// <summary>
    /// Save analysis asynchronously.
    /// </summary>
    public Task<QueryAnalysisResult> SaveAnalysisAsync(QueryAnalysisResult analysis)
    {
        lock (_lock)
        {
            var existing = _analyses.FirstOrDefault(a => a.QueryId == analysis.QueryId);
            if (existing != null)
                _analyses.Remove(existing);
            _analyses.Add(analysis);
            return Task.FromResult(analysis);
        }
    }

    /// <summary>
    /// Delete analysis asynchronously.
    /// </summary>
    public Task DeleteAnalysisAsync(string queryId)
    {
        lock (_lock)
        {
            _analyses.RemoveAll(a => a.QueryId == queryId);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Get issues by type asynchronously.
    /// </summary>
    public Task<List<PerformanceIssue>> GetIssuesByTypeAsync(IssueType issueType)
    {
        lock (_lock)
        {
            var issues = _analyses
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
            var issues = _analyses
                .SelectMany(a => a.Issues)
                .Where(i => i.Severity == IssueSeverity.Critical)
                .OrderByDescending(i => i.EstimatedPerformanceImpact)
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
            return Task.FromResult(_analyses.Sum(a => a.Issues.Count));
        }
    }

    // New method for IAnalysisRepository.GetAnalysesForQueryAsync
    /// <summary>
    /// Get analyses for query asynchronously.
    /// </summary>
    public Task<List<QueryAnalysisResult>> GetAnalysesForQueryAsync(string queryHash)
    {
        lock (_lock)
        {
            var results = _analyses
                .Where(a => string.Equals(a.Query, queryHash, StringComparison.Ordinal) || string.Equals(a.QueryId, queryHash, StringComparison.Ordinal))
                .ToList();
            return Task.FromResult(results);
        }
    }

    // New method for IAnalysisRepository.GetRecentAnalysesAsync
    /// <summary>
    /// Get recent analyses asynchronously.
    /// </summary>
    public Task<List<QueryAnalysisResult>> GetRecentAnalysesAsync(int count = 100)
    {
        lock (_lock)
        {
            var results = _analyses
                .OrderByDescending(a => a.AnalyzedAt)
                .Take(count)
                .ToList();
            return Task.FromResult(results);
        }
    }
}

/// <summary>
/// In-memory implementation of index repository
/// </summary>
public class IndexRepository : IIndexRepository
{
    private readonly List<ModelIndex> _indexes = [];
    private readonly List<IndexSuggestion> _suggestions = [];
    private readonly object _lock = new();

    /// <summary>
    /// Get index by name asynchronously.
    /// </summary>
    public Task<ModelIndex?> GetIndexByNameAsync(string indexName)
    {
        lock (_lock)
        {
            var index = _indexes.FirstOrDefault(i => i.IndexName == indexName);
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
            var indexes = _indexes.Where(i => i.TableName == tableName).ToList();
            return Task.FromResult(indexes);
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
            var indexes = _indexes
                .Where(i => string.Equals(i.TableName, tableName, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return Task.FromResult(indexes);
        }
    }

    /// <summary>
    /// Get index asynchronously.
    /// </summary>
    public Task<ModelIndex?> GetIndexAsync(string indexId)
    {
        lock (_lock)
        {
            var index = _indexes.FirstOrDefault(i => i.IndexId == indexId);
            return Task.FromResult(index);
        }
    }

    /// <summary>
    /// Get all indexes asynchronously.
    /// </summary>
    public Task<List<ModelIndex>> GetAllIndexesAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(new List<ModelIndex>(_indexes));
        }
    }

    /// <summary>
    /// Get unused indexes asynchronously.
    /// </summary>
    public Task<List<ModelIndex>> GetUnusedIndexesAsync()
    {
        lock (_lock)
        {
            var unused = _indexes.Where(i => i.IsCandidateForRemoval).ToList();
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
            var fragmented = _indexes.Where(i => i.IsFragmented).ToList();
            return Task.FromResult(fragmented);
        }
    }

    /// <summary>
    /// Add index asynchronously.
    /// </summary>
    public Task<ModelIndex> AddIndexAsync(ModelIndex index)
    {
        lock (_lock)
        {
            _indexes.Add(index);
            return Task.FromResult(index);
        }
    }

    /// <summary>
    /// Update index asynchronously.
    /// </summary>
    public Task UpdateIndexAsync(ModelIndex index)
    {
        lock (_lock)
        {
            var existing = _indexes.FirstOrDefault(i => i.IndexId == index.IndexId);
            if (existing != null)
            {
                _indexes.Remove(existing);
                _indexes.Add(index);
            }
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Delete index asynchronously.
    /// </summary>
    public Task DeleteIndexAsync(string indexId)
    {
        lock (_lock)
        {
            _indexes.RemoveAll(i => i.IndexId == indexId);
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
