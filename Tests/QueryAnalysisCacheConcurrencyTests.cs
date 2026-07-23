#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Utilities;
using SqlQueryAnalyzer.Caching;

namespace SqlQueryAnalyzer.Tests;

/// <summary>
/// Concurrency stress tests for QueryAnalysisCache to verify thread safety
/// under high concurrent access scenarios.
/// </summary>
public class QueryAnalysisCacheConcurrencyTests
{
    private readonly QueryAnalysisCache _cache;
    private readonly QueryCacheKeyGenerator _keyGenerator;
    private readonly ILogger<QueryAnalysisCache> _logger;

    public QueryAnalysisCacheConcurrencyTests()
    {
        // Setup minimal dependencies for testing
        _keyGenerator = new QueryCacheKeyGenerator();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning); // Reduce noise in test output
        });
        _logger = loggerFactory.CreateLogger<QueryAnalysisCache>();

        _cache = new QueryAnalysisCache(_logger, _keyGenerator, maxEntries: 100, ttlSeconds: 60);
    }

    /// <summary>
    /// Tests concurrent GetOrAdd operations from multiple threads.
    /// Verifies that cache hits are consistent and no race conditions occur.
    /// </summary>
    [Fact]
    public void ConcurrentGetOrAdd_ShouldBeThreadSafe()
    {
        // Arrange
        const int threadCount = 20;
        const int operationsPerThread = 100;
        var query = "SELECT * FROM Users WHERE Status = 'Active'";
        var results = new ConcurrentBag<QueryAnalysisResult>();
        var exceptions = new ConcurrentBag<Exception>();

        // Act - multiple threads adding the same query simultaneously
        var threads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        var result = _cache.GetOrAdd(query, q => CreateAnalysisResult(q, j));
                        results.Add(result);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });
        }

        // Start all threads
        foreach (var thread in threads)
        {
            thread.Start();
        }

        // Wait for completion
        foreach (var thread in threads)
        {
            thread.Join();
        }

        // Assert
        Assert.Empty(exceptions);
        Assert.Equal(threadCount * operationsPerThread, results.Count);

        // All results should be identical (same query)
        var firstResult = results.First();
        foreach (var result in results)
        {
            Assert.Equal(firstResult.Query, result.Query);
            Assert.Equal(firstResult.Issues.Count, result.Issues.Count);
        }

        // Cache should have exactly one entry (all threads accessed the same cached result)
        Assert.Equal(1, _cache.Count);
    }

    /// <summary>
    /// Tests concurrent access with different queries to verify LRU eviction works correctly
    /// under high load.
    /// </summary>
    [Fact]
    public void ConcurrentAccessWithDifferentQueries_ShouldHandleLRUEviction()
    {
        // Arrange
        const int threadCount = 10;
        const int queriesPerThread = 50;
        var exceptions = new ConcurrentBag<Exception>();
        var uniqueQueries = new ConcurrentBag<string>();

        // Generate unique queries for each operation
        for (int i = 0; i < threadCount * queriesPerThread; i++)
        {
            uniqueQueries.Add($"SELECT * FROM Table{i} WHERE Id = {i}");
        }

        // Act - multiple threads accessing different queries
        var threads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    int queryIndex = 0;
                    foreach (var query in uniqueQueries)
                    {
                        if (queryIndex % threadCount == threadId)
                        {
                            var result = _cache.GetOrAdd(query, q => CreateAnalysisResult(q, threadId));
                            Thread.Sleep(1); // Small delay to increase chance of interleaving
                        }
                        queryIndex++;
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });
        }

        // Start all threads
        foreach (var thread in threads)
        {
            thread.Start();
        }

        // Wait for completion
        foreach (var thread in threads)
        {
            thread.Join();
        }

        // Assert
        Assert.Empty(exceptions);

        // Cache should not exceed max entries significantly (allow some slack for eviction timing)
        Assert.InRange(_cache.Count, 50, 110);
    }

    /// <summary>
    /// Tests concurrent Invalidate operations.
    /// </summary>
    [Fact]
    public void ConcurrentInvalidate_ShouldBeThreadSafe()
    {
        // Arrange - populate cache first
        const int initialEntries = 50;
        for (int i = 0; i < initialEntries; i++)
        {
            var query = $"SELECT * FROM Test{i}";
            _cache.Set(query, CreateAnalysisResult(query, i));
        }

        var exceptions = new ConcurrentBag<Exception>();
        var threads = new Thread[10];

        // Act - multiple threads invalidating different entries
        for (int i = 0; i < threads.Length; i++)
        {
            int index = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    var query = $"SELECT * FROM Test{index * 5}";
                    _cache.Invalidate(query);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });
        }

        // Start all threads
        foreach (var thread in threads)
        {
            thread.Start();
        }

        // Wait for completion
        foreach (var thread in threads)
        {
            thread.Join();
        }

        // Assert
        Assert.Empty(exceptions);

        // Cache should have reduced count (some entries were invalidated)
        var stats = _cache.GetStatistics();
        Assert.True(stats.TotalEntries < initialEntries);
    }

    /// <summary>
    /// Tests concurrent Clear operations.
    /// </summary>
    [Fact]
    public void ConcurrentClear_ShouldBeThreadSafe()
    {
        // Arrange - populate cache
        for (int i = 0; i < 20; i++)
        {
            var query = $"SELECT * FROM Test{i}";
            _cache.Set(query, CreateAnalysisResult(query, i));
        }

        var exceptions = new ConcurrentBag<Exception>();
        var threads = new Thread[5];

        // Act - multiple threads trying to clear
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() =>
            {
                try
                {
                    _cache.Clear();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });
        }

        // Start all threads
        foreach (var thread in threads)
        {
            thread.Start();
        }

        // Wait for completion
        foreach (var thread in threads)
        {
            thread.Join();
        }

        // Assert
        Assert.Empty(exceptions);
        Assert.Equal(0, _cache.Count);
    }

    /// <summary>
    /// Tests concurrent statistics collection.
    /// </summary>
    [Fact]
    public void ConcurrentGetStatistics_ShouldBeThreadSafe()
    {
        // Arrange - populate cache
        for (int i = 0; i < 30; i++)
        {
            var query = $"SELECT * FROM StatsTest{i}";
            _cache.Set(query, CreateAnalysisResult(query, i));
        }

        var exceptions = new ConcurrentBag<Exception>();
        var threads = new Thread[15];

        // Act - multiple threads getting statistics
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < 10; j++)
                    {
                        var stats = _cache.GetStatistics();
                        Assert.NotNull(stats);
                        Assert.InRange(stats.TotalEntries, 0, 100);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });
        }

        // Start all threads
        foreach (var thread in threads)
        {
            thread.Start();
        }

        // Wait for completion
        foreach (var thread in threads)
        {
            thread.Join();
        }

        // Assert
        Assert.Empty(exceptions);
    }

    /// <summary>
    /// Tests very high concurrency scenario (100 threads).
    /// </summary>
    [Fact]
    public void HighConcurrency_100Threads_ShouldNotFail()
    {
        // Arrange
        const int threadCount = 100;
        const int operationsPerThread = 50;
        var exceptions = new ConcurrentBag<Exception>();
        var results = new ConcurrentBag<QueryAnalysisResult>();

        // Act
        var threads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        var query = $"SELECT * FROM ConcurrentTest{threadId % 10}"; // 10 unique queries
                        var result = _cache.GetOrAdd(query, q => CreateAnalysisResult(q, threadId));
                        results.Add(result);
                        Thread.Yield(); // Encourage context switching
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });
        }

        // Start all threads
        foreach (var thread in threads)
        {
            thread.Start();
        }

        // Wait for completion
        foreach (var thread in threads)
        {
            thread.Join(TimeSpan.FromSeconds(30));
        }

        // Assert
        Assert.Empty(exceptions);
        Assert.Equal(threadCount * operationsPerThread, results.Count);
        Assert.InRange(_cache.Count, 1, 20); // Should have at most 10 unique entries
    }

    /// <summary>
    /// Creates a test analysis result.
    /// </summary>
    private static QueryAnalysisResult CreateAnalysisResult(string query, int seed)
    {
        return new QueryAnalysisResult
        {
            Query = query,
            QueryId = Guid.NewGuid().ToString(),
            AnalyzedAt = DateTime.UtcNow,
            PerformanceScore = 85.0 - (seed % 20),
            Issues = [
                new PerformanceIssue
                {
                    IssueType = "Performance",
                    Description = $"Test issue {seed}",
                    Severity = IssueSeverity.Warning
                }
            ],
            IndexSuggestions = [
                new IndexSuggestion
                {
                    TableName = "TestTable",
                    ColumnName = "TestColumn",
                    EstimatedPerformanceGain = 15.5
                }
            ]
        };
    }
}