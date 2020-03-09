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
public class QueryNormalizerBenchmarks
{
    private QueryNormalizer _normalizer = null!;

    private const string SimpleQuery =
        "select * from orders where customerid = 1 and status = 'active'";

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

    private const string QueryWithStringLiterals = @"
        SELECT * FROM Products
        WHERE Name LIKE '%Widget%'
          AND Description = 'It''s a great product'
          AND Category IN ('Electronics', 'Gadgets', 'Tech''s Best')
        ORDER BY Price";

    [GlobalSetup]
    public void Setup() => _normalizer = new QueryNormalizer();

    [BenchmarkCategory("Normalize"), Benchmark(Description = "Simple SELECT — normalize whitespace + keywords")]
    public string NormalizeSimple() => _normalizer.Normalize(SimpleQuery);

    [BenchmarkCategory("Normalize"), Benchmark(Description = "Complex 5-JOIN query — full normalization pipeline")]
    public string NormalizeComplex() => _normalizer.Normalize(ComplexQuery);

    [BenchmarkCategory("Normalize"), Benchmark(Description = "Query with embedded string literals — extract + restore")]
    public string NormalizeWithLiterals() => _normalizer.Normalize(QueryWithStringLiterals);

    [BenchmarkCategory("Extract"), Benchmark(Description = "Extract table names from 5-JOIN query")]
    public List<string> ExtractTableNamesComplex() => _normalizer.ExtractTableNames(ComplexQuery);

    [BenchmarkCategory("Extract"), Benchmark(Description = "Extract column names from SELECT clause")]
    public List<string> ExtractColumnNamesComplex() => _normalizer.ExtractColumnNames(ComplexQuery);
}
