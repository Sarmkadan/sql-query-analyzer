#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class SqlPatternAnalyzerBenchmarks
{
    private List<string> _repeatedQueries = null!;
    private List<string> _diverseQueries = null!;

    private const string CleanQuery =
        "SELECT Id, Name, Price FROM Products WHERE CategoryId = 5 ORDER BY Price DESC";

    private const string ProblematicQuery = @"
        SELECT * FROM Orders o, Customers c
        WHERE UPPER(c.Country) = 'USA'
          AND o.Status LIKE '%active%'
          OR  o.Type = 'standard'
          OR  o.Priority = 'high'";

    private const string ComplexNestedQuery = @"
        SELECT * FROM (
            SELECT o.OrderId, COUNT(*) cnt
            FROM Orders o
            WHERE YEAR(o.OrderDate) = 2024
              AND MONTH(o.OrderDate) IN (1, 2, 3)
            GROUP BY o.OrderId
        ) sub
        WHERE sub.cnt > 5";

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

    [BenchmarkCategory("Detection"), Benchmark(Description = "N+1 detection — 20 queries hitting same table")]
    public bool DetectNPlusOneRepeated() => SqlPatternAnalyzer.DetectNPlusOnePattern(_repeatedQueries);

    [BenchmarkCategory("Detection"), Benchmark(Description = "N+1 detection — 6 diverse queries")]
    public bool DetectNPlusOneDiverse() => SqlPatternAnalyzer.DetectNPlusOnePattern(_diverseQueries);

    [BenchmarkCategory("Detection"), Benchmark(Description = "Extract tables from 2-table implicit JOIN query")]
    public List<string> ExtractTablesProblematic() => SqlPatternAnalyzer.ExtractTablesFromQuery(ProblematicQuery);

    [BenchmarkCategory("Detection"), Benchmark(Description = "Extract tables from nested subquery")]
    public List<string> ExtractTablesNested() => SqlPatternAnalyzer.ExtractTablesFromQuery(ComplexNestedQuery);

    [BenchmarkCategory("Analysis"), Benchmark(Description = "Full optimization recommendations — clean query")]
    public List<string> RecommendationsClean() => SqlPatternAnalyzer.GenerateOptimizationRecommendations(CleanQuery);

    [BenchmarkCategory("Analysis"), Benchmark(Description = "Full optimization recommendations — problematic query")]
    public List<string> RecommendationsProblematic() => SqlPatternAnalyzer.GenerateOptimizationRecommendations(ProblematicQuery);

    [BenchmarkCategory("Analysis"), Benchmark(Description = "Readability score — problematic query with OR + wildcards")]
    public double ReadabilityScoreProblematic() => SqlPatternAnalyzer.CalculateReadabilityScore(ProblematicQuery);

    [BenchmarkCategory("Analysis"), Benchmark(Description = "Count parentheses nesting depth")]
    public int CountParenthesesNested() => SqlPatternAnalyzer.CountParentheses(ComplexNestedQuery);

    [BenchmarkCategory("Checks"), Benchmark(Description = "HasFunctionOnColumn — WHERE with UPPER/YEAR/MONTH")]
    public bool HasFunctionOnColumn() => SqlPatternAnalyzer.HasFunctionOnColumn(ComplexNestedQuery);

    [BenchmarkCategory("Checks"), Benchmark(Description = "CountOrConditions — 2 OR branches")]
    public int CountOrConditions() => SqlPatternAnalyzer.CountOrConditions(ProblematicQuery);
}
