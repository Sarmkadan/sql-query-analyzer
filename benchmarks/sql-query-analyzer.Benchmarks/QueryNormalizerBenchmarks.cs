#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Benchmarks;

/// <summary>
/// Provides benchmarks for the QueryNormalizer class.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class QueryNormalizerBenchmarks
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryNormalizerBenchmarks"/> class.
    /// </summary>
    private QueryNormalizer _normalizer = null!;

    /// <summary>
    /// A simple SELECT query with whitespace and keywords.
    /// </summary>
    private const string SimpleQuery =
        "select * from orders where customerid = 1 and status = 'active'";

    /// <summary>
    /// A complex 5-JOIN query with various clauses.
    /// </summary>
    private const string ComplexQuery = @"
        SELECT  o.OrderId,o.OrderDate,c.CustomerName,p.ProductName,od.Quantity,od.UnitPrice,
                cat.CategoryName,  SUM(od.Quantity * od.UnitPrice) as LineTotal
        FROM    Orders o
        INNER   JOIN Customers c on o.CustomerId=c.CustomerId
        LEFT    JOIN OrderDetails od ON o.OrderId = od.OrderId
        LEFT    JOIN Products p   ON od.ProductId=p.ProductId
        INNER   JOIN Categories cat ON p.CategoryId = cat.CategoryId
        WHERE   o.OrderDate >= '2024-01-01'
          and   o.Status = 'Active'
          AND   c.Country IN ('USA', 'UK', 'Canada')
        group by o.OrderId, o.OrderDate, c.CustomerName, p.ProductName,
                 od.Quantity, od.UnitPrice, cat.CategoryName
        order by o.OrderDate DESC";

    /// <summary>
    /// A query with embedded string literals.
    /// </summary>
    private const string QueryWithStringLiterals = @"
        SELECT * FROM Products
        WHERE Name LIKE '%Widget%'
          AND Description = 'It''s a great product'
          AND Category IN ('Electronics', 'Gadgets', 'Tech''s Best')
        ORDER BY Price";

    /// <summary>
    /// Initializes the QueryNormalizer instance used for benchmarking.
    /// </summary>
    [GlobalSetup]
    public void Setup() => _normalizer = new QueryNormalizer();

    /// <summary>
    /// Normalizes the whitespace and keywords in a simple SELECT query.
    /// </summary>
    /// <returns>The normalized query.</returns>
    [BenchmarkCategory("Normalize"), Benchmark(Description = "Simple SELECT — normalize whitespace + keywords")]
    public string NormalizeSimple() => _normalizer.Normalize(SimpleQuery);

    /// <summary>
    /// Normalizes the complex 5-JOIN query with various clauses.
    /// </summary>
    /// <returns>The normalized query.</returns>
    [BenchmarkCategory("Normalize"), Benchmark(Description = "Complex 5-JOIN query — full normalization pipeline")]
    public string NormalizeComplex() => _normalizer.Normalize(ComplexQuery);

    /// <summary>
    /// Normalizes the query with embedded string literals.
    /// </summary>
    /// <returns>The normalized query.</returns>
    [BenchmarkCategory("Normalize"), Benchmark(Description = "Query with embedded string literals — extract + restore")]
    public string NormalizeWithLiterals() => _normalizer.Normalize(QueryWithStringLiterals);

    /// <summary>
    /// Extracts the table names from a complex 5-JOIN query.
    /// </summary>
    /// <returns>A list of extracted table names.</returns>
    [BenchmarkCategory("Extract"), Benchmark(Description = "Extract table names from 5-JOIN query")]
    public List<string> ExtractTableNamesComplex() => _normalizer.ExtractTableNames(ComplexQuery);

    /// <summary>
    /// Extracts the column names from the SELECT clause of a complex 5-JOIN query.
    /// </summary>
    /// <returns>A list of extracted column names.</returns>
    [BenchmarkCategory("Extract"), Benchmark(Description = "Extract column names from SELECT clause")]
    public List<string> ExtractColumnNamesComplex() => _normalizer.ExtractColumnNames(ComplexQuery);
}
