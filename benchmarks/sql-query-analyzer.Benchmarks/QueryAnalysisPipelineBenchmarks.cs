#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Benchmarks;

/// <summary>
/// Benchmarks for the full query parsing and analysis pipeline.
/// Covers DatabaseQuery.Parse() (type detection + regex extraction), SHA-256 hashing,
/// and combined pattern checks representative of a real analysis pass.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class QueryAnalysisPipelineBenchmarks
{
    private const string SimpleQueryText =
        "SELECT Id, Name, UnitPrice FROM Products WHERE CategoryId = 5 AND IsActive = 1";

    private const string ComplexQueryText = @"
        SELECT  o.OrderId, o.OrderDate, c.CustomerName,
                p.ProductName, od.Quantity, od.UnitPrice,
                cat.CategoryName
        FROM    Orders o
        INNER   JOIN Customers    c   ON o.CustomerId   = c.CustomerId
        LEFT    JOIN OrderDetails od  ON o.OrderId      = od.OrderId
        LEFT    JOIN Products     p   ON od.ProductId   = p.ProductId
        INNER   JOIN Categories   cat ON p.CategoryId   = cat.CategoryId
        WHERE   o.OrderDate >= '2024-01-01'
          AND   c.Country = 'USA'
          AND   o.Status  = 'Active'
        ORDER   BY o.OrderDate DESC";

    private const string StoredProcQueryText = @"
        DECLARE @CustomerId INT = 42;
        DECLARE @StartDate  DATETIME = '2024-01-01';

        SELECT  c.CustomerName,
                SUM(od.Quantity * od.UnitPrice) AS TotalRevenue,
                COUNT(DISTINCT o.OrderId)       AS OrderCount
        FROM    Customers    c
        INNER   JOIN Orders       o  ON c.CustomerId = o.CustomerId
        INNER   JOIN OrderDetails od ON o.OrderId    = od.OrderId
        WHERE   c.CustomerId = @CustomerId
          AND   o.OrderDate  >= @StartDate
        GROUP   BY c.CustomerName
        HAVING  SUM(od.Quantity * od.UnitPrice) > 1000
        ORDER   BY TotalRevenue DESC;";

    [BenchmarkCategory("Parse"), Benchmark(Description = "Parse simple SELECT — type + table extraction")]
    public void ParseSimpleQuery()
    {
        var q = new DatabaseQuery { QueryText = SimpleQueryText };
        q.Parse();
    }

    [BenchmarkCategory("Parse"), Benchmark(Description = "Parse 4-JOIN query — type + table + join extraction")]
    public void ParseComplexQuery()
    {
        var q = new DatabaseQuery { QueryText = ComplexQueryText };
        q.Parse();
    }

    [BenchmarkCategory("Parse"), Benchmark(Description = "Parse stored procedure with DECLARE + GROUP BY + HAVING")]
    public void ParseStoredProcQuery()
    {
        var q = new DatabaseQuery { QueryText = StoredProcQueryText };
        q.Parse();
    }

    [BenchmarkCategory("Hash"), Benchmark(Description = "Parse + SHA-256 hash — simple query")]
    public string HashSimpleQuery()
    {
        var q = new DatabaseQuery { QueryText = SimpleQueryText };
        q.Parse();
        return q.GenerateHash();
    }

    [BenchmarkCategory("Hash"), Benchmark(Description = "Parse + SHA-256 hash — complex query")]
    public string HashComplexQuery()
    {
        var q = new DatabaseQuery { QueryText = ComplexQueryText };
        q.Parse();

        [BenchmarkCategory("Parse"), Benchmark(Description = "Parse simple SELECT — validation helper")]
        public void ParseSimpleQueryBenchmark()
        {
            var q = new DatabaseQuery { QueryText = SimpleQueryText };
            q.Parse();
        }

        [BenchmarkCategory("Parse"), Benchmark(Description = "Parse 4-JOIN query — validation helper")]
        public void ParseComplexQueryBenchmark()
        {
            var q = new DatabaseQuery { QueryText = ComplexQueryText };
            q.Parse();
        }

        [BenchmarkCategory("Parse"), Benchmark(Description = "Parse stored procedure — validation helper")]
        public void ParseStoredProcQueryBenchmark()
        {
            var q = new DatabaseQuery { QueryText = StoredProcQueryText };
            q.Parse();
        }

        [BenchmarkCategory("Hash"), Benchmark(Description = "Parse + SHA-256 hash — simple query validation")]
        public string HashSimpleQueryBenchmark()
        {
            var q = new DatabaseQuery { QueryText = SimpleQueryText };
            q.Parse();
            return q.GenerateHash();
        }

        [BenchmarkCategory("Hash"), Benchmark(Description = "Parse + SHA-256 hash — complex query validation")]
        public string HashComplexQueryBenchmark()
        {
            var q = new DatabaseQuery { QueryText = ComplexQueryText };
            q.Parse();
            return q.GenerateHash();
        }

        [BenchmarkCategory("Combined"), Benchmark(Description = "Full pattern suite — validation helper")]
        public (bool selectStar, bool funcOnCol, bool leadingWildcard, int orCount, bool subquery, int caseCount, bool aggregate) FullPatternSuiteBenchmark()
        {
            var query = ComplexQueryText;
            return (
                SqlPatternAnalyzer.HasSelectStar(query),
                SqlPatternAnalyzer.HasFunctionOnColumn(query),
                SqlPatternAnalyzer.HasLeadingWildcardLike(query),
                SqlPatternAnalyzer.CountOrConditions(query),
                SqlPatternAnalyzer.HasSubquery(query),
                SqlPatternAnalyzer.CountCaseStatements(query),
                SqlPatternAnalyzer.HasAggregateFunction(query)
            );
        }

        [BenchmarkCategory("Combined"), Benchmark(Description = "Extract join conditions — validation helper")]
        public List<string> ExtractJoinConditionsBenchmark() =>
            SqlPatternAnalyzer.ExtractJoinConditions(ComplexQueryText);
    }
