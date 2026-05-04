// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SqlQueryAnalyzer.Models;

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
    Task SaveAnalysisAsync(QueryAnalysisResult result);

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

    public Task SaveAnalysisAsync(QueryAnalysisResult result)
    {
        lock (_lock)
        {
            _analyses[result.QueryId] = result;
        }

        return Task.CompletedTask;
    }

    public Task<QueryAnalysisResult?> GetAnalysisAsync(string analysisId)
    {
        lock (_lock)
        {
            _analyses.TryGetValue(analysisId, out var result);
            return Task.FromResult(result);
        }
    }

    public Task<List<QueryAnalysisResult>> GetAnalysesForQueryAsync(string queryHash)
    {
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
}

/// <summary>
/// Repository interface for storing indexes.
/// </summary>
public interface IIndexRepository
{
    Task SaveIndexAsync(Index index);
    Task<List<Index>> GetIndexesForTableAsync(string tableName);
    Task<Index?> GetIndexAsync(string indexId);
    Task DeleteIndexAsync(string indexId);
}

/// <summary>
/// In-memory implementation of index repository.
/// </summary>
public class InMemoryIndexRepository : IIndexRepository
{
    private readonly Dictionary<string, Index> _indexes = new();
    private readonly object _lock = new object();

    public Task SaveIndexAsync(Index index)
    {
        lock (_lock)
        {
            _indexes[index.IndexId] = index;
        }

        return Task.CompletedTask;
    }

    public Task<List<Index>> GetIndexesForTableAsync(string tableName)
    {
        lock (_lock)
        {
            var results = _indexes.Values
                .Where(i => i.TableName == tableName)
                .ToList();

            return Task.FromResult(results);
        }
    }

    public Task<Index?> GetIndexAsync(string indexId)
    {
        lock (_lock)
        {
            _indexes.TryGetValue(indexId, out var index);
            return Task.FromResult(index);
        }
    }

    public Task DeleteIndexAsync(string indexId)
    {
        lock (_lock)
        {
            _indexes.Remove(indexId);
        }

        return Task.CompletedTask;
    }
}
