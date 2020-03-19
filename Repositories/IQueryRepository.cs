#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Repositories;

/// <summary>
/// Repository interface for query operations
/// </summary>
public interface IQueryRepository
{
    Task<DatabaseQuery?> GetQueryByIdAsync(string queryId);
    Task<List<DatabaseQuery>> GetAllQueriesAsync();
    Task<List<DatabaseQuery>> GetQueriesByTableAsync(string tableName);
    Task<List<DatabaseQuery>> GetQueriesByTypeAsync(QueryType queryType);
    Task<DatabaseQuery> AddQueryAsync(DatabaseQuery query);
    Task UpdateQueryAsync(DatabaseQuery query);
    Task DeleteQueryAsync(string queryId);
    Task<List<DatabaseQuery>> SearchQueriesAsync(string searchTerm);
    Task<List<DatabaseQuery>> GetQueriesByApplicationAsync(string applicationName);
    Task<int> GetQueryCountAsync();
}

/// <summary>
/// Repository interface for analysis results
/// </summary>
public interface IAnalysisRepository
{
    Task<QueryAnalysisResult?> GetAnalysisAsync(string queryId);
    Task<List<QueryAnalysisResult>> GetAllAnalysesAsync();
    Task<List<QueryAnalysisResult>> GetAnalysesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<QueryAnalysisResult> SaveAnalysisAsync(QueryAnalysisResult analysis);
    Task DeleteAnalysisAsync(string queryId);
    Task<List<PerformanceIssue>> GetIssuesByTypeAsync(IssueType issueType);
    Task<List<PerformanceIssue>> GetCriticalIssuesAsync();
    Task<int> GetTotalIssueCountAsync();
}

/// <summary>
/// Repository interface for index operations
/// </summary>
public interface IIndexRepository
{
    Task<Index?> GetIndexByNameAsync(string indexName);
    Task<List<Index>> GetIndexesByTableAsync(string tableName);
    Task<List<Index>> GetAllIndexesAsync();
    Task<List<Index>> GetUnusedIndexesAsync();
    Task<List<Index>> GetFragmentedIndexesAsync();
    Task<Index> AddIndexAsync(Index index);
    Task UpdateIndexAsync(Index index);
    Task DeleteIndexAsync(string indexId);
    Task<List<IndexSuggestion>> GetSuggestionsAsync();
    Task SaveSuggestionAsync(IndexSuggestion suggestion);
    Task<int> GetIndexCountAsync();
}

/// <summary>
/// In-memory implementation of query repository for Phase 1
/// </summary>
public class QueryRepository : IQueryRepository
{
    private readonly List<DatabaseQuery> _queries = [];
    private readonly object _lock = new();

    public Task<DatabaseQuery?> GetQueryByIdAsync(string queryId)
    {
        lock (_lock)
        {
            var query = _queries.FirstOrDefault(q => q.QueryId == queryId);
            return Task.FromResult(query);
        }
    }

    public Task<List<DatabaseQuery>> GetAllQueriesAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(new List<DatabaseQuery>(_queries));
        }
    }

    public Task<List<DatabaseQuery>> GetQueriesByTableAsync(string tableName)
    {
        lock (_lock)
        {
            var queries = _queries
                .Where(q => q.ReferencedTables.Contains(tableName, StringComparer.OrdinalIgnoreCase))
                .ToList();
            return Task.FromResult(queries);
        }
    }

    public Task<List<DatabaseQuery>> GetQueriesByTypeAsync(QueryType queryType)
    {
        lock (_lock)
        {
            var queries = _queries.Where(q => q.QueryType == queryType).ToList();
            return Task.FromResult(queries);
        }
    }

    public Task<DatabaseQuery> AddQueryAsync(DatabaseQuery query)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(query.QueryHash))
                query.GenerateHash();
            _queries.Add(query);
            return Task.FromResult(query);
        }
    }

    public Task UpdateQueryAsync(DatabaseQuery query)
    {
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

    public Task DeleteQueryAsync(string queryId)
    {
        lock (_lock)
        {
            _queries.RemoveAll(q => q.QueryId == queryId);
            return Task.CompletedTask;
        }
    }

    public Task<List<DatabaseQuery>> SearchQueriesAsync(string searchTerm)
    {
        lock (_lock)
        {
            var results = _queries
                .Where(q => q.QueryText.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           q.ProcedureName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
            return Task.FromResult(results);
        }
    }

    public Task<List<DatabaseQuery>> GetQueriesByApplicationAsync(string applicationName)
    {
        lock (_lock)
        {
            var queries = _queries
                .Where(q => q.ApplicationName == applicationName)
                .ToList();
            return Task.FromResult(queries);
        }
    }

    public Task<int> GetQueryCountAsync()
    {
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

    public Task<QueryAnalysisResult?> GetAnalysisAsync(string queryId)
    {
        lock (_lock)
        {
            var analysis = _analyses.FirstOrDefault(a => a.QueryId == queryId);
            return Task.FromResult(analysis);
        }
    }

    public Task<List<QueryAnalysisResult>> GetAllAnalysesAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(new List<QueryAnalysisResult>(_analyses));
        }
    }

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

    public Task DeleteAnalysisAsync(string queryId)
    {
        lock (_lock)
        {
            _analyses.RemoveAll(a => a.QueryId == queryId);
            return Task.CompletedTask;
        }
    }

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

    public Task<List<PerformanceIssue>> GetCriticalIssuesAsync()
    {
        lock (_lock)
        {
            var issues = _analyses
                .SelectMany(a => a.Issues)
                .Where(i => i.IsCritical)
                .OrderByDescending(i => i.EstimatedPerformanceImpact)
                .ToList();
            return Task.FromResult(issues);
        }
    }

    public Task<int> GetTotalIssueCountAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_analyses.Sum(a => a.Issues.Count));
        }
    }
}

/// <summary>
/// In-memory implementation of index repository
/// </summary>
public class IndexRepository : IIndexRepository
{
    private readonly List<Index> _indexes = [];
    private readonly List<IndexSuggestion> _suggestions = [];
    private readonly object _lock = new();

    public Task<Index?> GetIndexByNameAsync(string indexName)
    {
        lock (_lock)
        {
            var index = _indexes.FirstOrDefault(i => i.IndexName == indexName);
            return Task.FromResult(index);
        }
    }

    public Task<List<Index>> GetIndexesByTableAsync(string tableName)
    {
        lock (_lock)
        {
            var indexes = _indexes.Where(i => i.TableName == tableName).ToList();
            return Task.FromResult(indexes);
        }
    }

    public Task<List<Index>> GetAllIndexesAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(new List<Index>(_indexes));
        }
    }

    public Task<List<Index>> GetUnusedIndexesAsync()
    {
        lock (_lock)
        {
            var unused = _indexes.Where(i => i.IsCandidateForRemoval).ToList();
            return Task.FromResult(unused);
        }
    }

    public Task<List<Index>> GetFragmentedIndexesAsync()
    {
        lock (_lock)
        {
            var fragmented = _indexes.Where(i => i.IsFragmented).ToList();
            return Task.FromResult(fragmented);
        }
    }

    public Task<Index> AddIndexAsync(Index index)
    {
        lock (_lock)
        {
            _indexes.Add(index);
            return Task.FromResult(index);
        }
    }

    public Task UpdateIndexAsync(Index index)
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

    public Task DeleteIndexAsync(string indexId)
    {
        lock (_lock)
        {
            _indexes.RemoveAll(i => i.IndexId == indexId);
            return Task.CompletedTask;
        }
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
