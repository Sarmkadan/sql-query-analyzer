#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Benchmarks;

/// <summary>
/// Provides benchmarks for the SqlPatternAnalyzer class.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class SqlPatternAnalyzerBenchmarks
{
    /// <summary>
    /// List of repeated queries used for N+1 pattern detection.
    /// </summary>
    private List<string> _repeatedQueries = null!;

    /// <summary>
    /// List of diverse queries used for N+1 pattern detection.
    /// </summary>
    private List<string> _diverseQueries = null!;

    /// <summary>
    /// A clean query used for optimization recommendations.
    /// </summary>
    private const string CleanQuery =
        "SELECT Id, Name, Price FROM Products WHERE CategoryId = 5 ORDER BY Price DESC";

    /// <summary>
    /// A problematic query used for optimization recommendations.
    /// </summary>
    private const string ProblematicQuery = @"
        SELECT * FROM Orders o, Customers c
        WHERE UPPER(c.Country) = 'USA'
          AND o.Status LIKE '%active%'
          OR  o.Type = 'standard'
          OR  o.Priority = 'high'";

    /// <summary>
    /// A complex nested query used for testing various features.
    /// </summary>
    private const string ComplexNestedQuery = @"
        SELECT * FROM (
            SELECT o.OrderId, COUNT(*) cnt
            FROM Orders o
            WHERE YEAR(o.OrderDate) = 2024
              AND MONTH(o.OrderDate) IN (1, 2, 3)
            GROUP BY o.OrderId
        ) sub
        WHERE sub.cnt > 5";

    /// <summary>
    /// Initializes the benchmark setup.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        // Simulates a typical N+1 pattern: same table accessed in a loop
        _repeatedQueries = Enumerable.Range(1, 20)
            .Select(i => $"SELECT OrderId, Total, Status FROM Orders WHERE CustomerId = {i}")
            .ToList();

        // Diverse query mix for more realistic batch detection
        _diverseQueries =
        [
            "SELECT * FROM Orders WHERE CustomerId = 1",
            "SELECT * FROM Orders WHERE CustomerId = 2",
            "SELECT Id FROM Customers WHERE Email = 'a@b.com'",
            "SELECT * FROM Orders WHERE CustomerId = 3",
            "INSERT INTO Logs (Message) VALUES ('test')",
            "SELECT * FROM Orders WHERE CustomerId = 4",
        ];
    }

    /// <summary>
    /// Detects N+1 pattern in a list of repeated queries.
    /// </summary>
    /// <returns>True if N+1 pattern is detected, false otherwise.</returns>
    [BenchmarkCategory("Detection"), Benchmark(Description = "N+1 detection — 20 queries hitting same table")]
    public bool DetectNPlusOneRepeated() => SqlPatternAnalyzer.DetectNPlusOnePattern(_repeatedQueries);

    /// <summary>
    /// Detects N+1 pattern in a list of diverse queries.
    /// </summary>
    /// <returns>True if N+1 pattern is detected, false otherwise.</returns>
    [BenchmarkCategory("Detection"), Benchmark(Description = "N+1 detection — 6 diverse queries")]
    public bool DetectNPlusOneDiverse() => SqlPatternAnalyzer.DetectNPlusOnePattern(_diverseQueries);

    /// <summary>
    /// Extracts tables from a 2-table implicit JOIN query.
    /// </summary>
    /// <returns>A list of extracted table names.</returns>
    [BenchmarkCategory("Detection"), Benchmark(Description = "Extract tables from 2-table implicit JOIN query")]
    public List<string> ExtractTablesProblematic() => SqlPatternAnalyzer.ExtractTablesFromQuery(ProblematicQuery);

    /// <summary>
    /// Extracts tables from a nested subquery.
    /// </summary>
    /// <returns>A list of extracted table names.</returns>
    [BenchmarkCategory("Detection"), Benchmark(Description = "Extract tables from nested subquery")]
    public List<string> ExtractTablesNested() => SqlPatternAnalyzer.ExtractTablesFromQuery(ComplexNestedQuery);

    /// <summary>
    /// Generates optimization recommendations for a clean query.
    /// </summary>
    /// <returns>A list of optimization recommendations.</returns>
    [BenchmarkCategory("Analysis"), Benchmark(Description = "Full optimization recommendations — clean query")]
    public List<string> RecommendationsClean() => SqlPatternAnalyzer.GenerateOptimizationRecommendations(CleanQuery);

    /// <summary>
    /// Generates optimization recommendations for a problematic query.
    /// </summary>
    /// <returns>A list of optimization recommendations.</returns>
    [BenchmarkCategory("Analysis"), Benchmark(Description = "Full optimization recommendations — problematic query")]
    public List<string> RecommendationsProblematic() => SqlPatternAnalyzer.GenerateOptimizationRecommendations(ProblematicQuery);

    /// <summary>
    /// Calculates the readability score for a problematic query with OR + wildcards.
    /// </summary>
    /// <returns>The readability score.</returns>
    [BenchmarkCategory("Analysis"), Benchmark(Description = "Readability score — problematic query with OR + wildcards")]
    public double ReadabilityScoreProblematic() => SqlPatternAnalyzer.CalculateReadabilityScore(ProblematicQuery);

    /// <summary>
    /// Counts the parentheses nesting depth in a complex nested query.
    /// </summary>
    /// <returns>The parentheses nesting depth.</returns>
    [BenchmarkCategory("Analysis"), Benchmark(Description = "Count parentheses nesting depth")]
    public int CountParenthesesNested() => SqlPatternAnalyzer.CountParentheses(ComplexNestedQuery);

    /// <summary>
    /// Checks if a query contains a function on a column.
    /// </summary>
    /// <returns>True if a function is found, false otherwise.</returns>
    [BenchmarkCategory("Checks"), Benchmark(Description = "HasFunctionOnColumn — WHERE with UPPER/YEAR/MONTH")]
    public bool HasFunctionOnColumn() => SqlPatternAnalyzer.HasFunctionOnColumn(ComplexNestedQuery);

    /// <summary>
    /// Counts the number of OR conditions in a query.
    /// </summary>
    /// <returns>The number of OR conditions.</returns>
    [BenchmarkCategory("Checks"), Benchmark(Description = "CountOrConditions — 2 OR branches")]
    public int CountOrConditions() => SqlPatternAnalyzer.CountOrConditions(ProblematicQuery);
}
