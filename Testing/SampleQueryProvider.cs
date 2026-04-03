// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SqlQueryAnalyzer.Testing;

/// <summary>
/// Provides sample SQL queries for testing and demonstration.
/// Includes queries with various performance characteristics and issues.
/// Useful for benchmarking, testing, and education.
/// </summary>
public static class SampleQueryProvider
{
    /// <summary>
    /// Gets a well-optimized query (high performance score).
    /// </summary>
    public static string GetOptimizedQuery() =>
        @"SELECT o.OrderId, o.OrderDate, c.CustomerName
          FROM Orders o
          INNER JOIN Customers c ON o.CustomerId = c.CustomerId
          WHERE o.OrderDate >= DATEADD(DAY, -30, GETDATE())
          ORDER BY o.OrderDate DESC";

    /// <summary>
    /// Gets a query with SELECT * (inefficiency).
    /// </summary>
    public static string GetSelectStarQuery() =>
        @"SELECT *
          FROM Orders o
          INNER JOIN Customers c ON o.CustomerId = c.CustomerId
          WHERE o.OrderStatus = 'Pending'";

    /// <summary>
    /// Gets a query with N+1 pattern.
    /// </summary>
    public static string GetNPlusOneQuery() =>
        @"SELECT c.CustomerId, c.CustomerName
          FROM Customers c
          WHERE c.Country = 'USA'
          -- Each customer result would trigger a separate query in real N+1
          -- SELECT COUNT(*) FROM Orders WHERE CustomerId = @CustomerId";

    /// <summary>
    /// Gets a query with implicit type conversion.
    /// </summary>
    public static string GetImplicitConversionQuery() =>
        @"SELECT p.ProductId, p.ProductName, p.Price
          FROM Products p
          WHERE p.ProductId = '100'"; // ProductId is numeric, comparing with string

    /// <summary>
    /// Gets a query with non-SARGable predicate.
    /// </summary>
    public static string GetNonSargableQuery() =>
        @"SELECT o.OrderId, o.OrderAmount
          FROM Orders o
          WHERE YEAR(o.OrderDate) = 2024"; // Function on column prevents index usage

    /// <summary>
    /// Gets a query with multiple joins.
    /// </summary>
    public static string GetComplexJoinQuery() =>
        @"SELECT o.OrderId, c.CustomerName, p.ProductName, s.StatusName
          FROM Orders o
          INNER JOIN Customers c ON o.CustomerId = c.CustomerId
          INNER JOIN OrderItems oi ON o.OrderId = oi.OrderId
          INNER JOIN Products p ON oi.ProductId = p.ProductId
          INNER JOIN OrderStatuses s ON o.StatusId = s.StatusId
          WHERE o.OrderDate >= DATEADD(MONTH, -3, GETDATE())
          ORDER BY o.OrderDate DESC";

    /// <summary>
    /// Gets a query with LIKE and leading wildcard.
    /// </summary>
    public static string GetLeadingWildcardQuery() =>
        @"SELECT c.CustomerId, c.CustomerName, c.Email
          FROM Customers c
          WHERE c.CustomerName LIKE '%Smith%'"; // Leading wildcard prevents index usage

    /// <summary>
    /// Gets a query with OR condition.
    /// </summary>
    public static string GetOrConditionQuery() =>
        @"SELECT p.ProductId, p.ProductName, p.Price
          FROM Products p
          WHERE p.CategoryId = 5 OR p.IsDiscounted = 1"; // OR may prevent index usage

    /// <summary>
    /// Gets a query with subquery.
    /// </summary>
    public static string GetSubqueryQuery() =>
        @"SELECT c.CustomerId, c.CustomerName
          FROM Customers c
          WHERE c.CustomerId IN (SELECT DISTINCT o.CustomerId FROM Orders o WHERE o.OrderAmount > 1000)";

    /// <summary>
    /// Gets a query with DISTINCT.
    /// </summary>
    public static string GetDistinctQuery() =>
        @"SELECT DISTINCT c.Country
          FROM Customers c
          INNER JOIN Orders o ON c.CustomerId = o.CustomerId
          WHERE o.OrderDate >= DATEADD(YEAR, -1, GETDATE())";

    /// <summary>
    /// Gets a simple query with no issues.
    /// </summary>
    public static string GetSimpleQuery() =>
        @"SELECT ProductId, ProductName, Price
          FROM Products
          WHERE Price > 100
          ORDER BY ProductName";

    /// <summary>
    /// Gets an aggregation query.
    /// </summary>
    public static string GetAggregationQuery() =>
        @"SELECT
            c.CustomerName,
            COUNT(o.OrderId) as OrderCount,
            SUM(o.OrderAmount) as TotalAmount,
            AVG(o.OrderAmount) as AverageAmount
          FROM Customers c
          LEFT JOIN Orders o ON c.CustomerId = o.CustomerId
          GROUP BY c.CustomerId, c.CustomerName
          HAVING COUNT(o.OrderId) > 5
          ORDER BY TotalAmount DESC";

    /// <summary>
    /// Gets a query with CTE (Common Table Expression).
    /// </summary>
    public static string GetCteQuery() =>
        @"WITH RecentOrders AS (
            SELECT CustomerId, OrderId, OrderAmount
            FROM Orders
            WHERE OrderDate >= DATEADD(MONTH, -1, GETDATE())
          )
          SELECT c.CustomerName, COUNT(ro.OrderId) as RecentOrderCount
          FROM Customers c
          LEFT JOIN RecentOrders ro ON c.CustomerId = ro.CustomerId
          GROUP BY c.CustomerId, c.CustomerName";

    /// <summary>
    /// Gets a very complex query.
    /// </summary>
    public static string GetVeryComplexQuery() =>
        @"WITH CustomerStats AS (
            SELECT
              CustomerId,
              COUNT(*) as OrderCount,
              SUM(OrderAmount) as TotalAmount,
              MAX(OrderDate) as LastOrderDate
            FROM Orders
            GROUP BY CustomerId
          ),
          RankedCustomers AS (
            SELECT
              c.CustomerId,
              c.CustomerName,
              cs.OrderCount,
              cs.TotalAmount,
              ROW_NUMBER() OVER (ORDER BY cs.TotalAmount DESC) as RankBySpend
            FROM Customers c
            LEFT JOIN CustomerStats cs ON c.CustomerId = cs.CustomerId
          )
          SELECT * FROM RankedCustomers
          WHERE RankBySpend <= 100
          ORDER BY RankBySpend";

    /// <summary>
    /// Gets all sample queries.
    /// </summary>
    public static Dictionary<string, string> GetAllSamples() =>
        new()
        {
            { "optimized", GetOptimizedQuery() },
            { "select_star", GetSelectStarQuery() },
            { "n_plus_one", GetNPlusOneQuery() },
            { "implicit_conversion", GetImplicitConversionQuery() },
            { "non_sargable", GetNonSargableQuery() },
            { "complex_join", GetComplexJoinQuery() },
            { "leading_wildcard", GetLeadingWildcardQuery() },
            { "or_condition", GetOrConditionQuery() },
            { "subquery", GetSubqueryQuery() },
            { "distinct", GetDistinctQuery() },
            { "simple", GetSimpleQuery() },
            { "aggregation", GetAggregationQuery() },
            { "cte", GetCteQuery() },
            { "very_complex", GetVeryComplexQuery() }
        };

    /// <summary>
    /// Gets a random sample query from the collection.
    /// </summary>
    public static string GetRandomSample()
    {
        var samples = GetAllSamples().Values.ToList();
        var random = new Random();
        return samples[random.Next(samples.Count)];
    }

    /// <summary>
    /// Gets sample queries grouped by issue type.
    /// </summary>
    public static Dictionary<string, List<string>> GetSamplesByIssueType() =>
        new()
        {
            { "Performance", new() { GetOptimizedQuery(), GetSimpleQuery() } },
            { "Missing Indexes", new() { GetLeadingWildcardQuery(), GetNonSargableQuery() } },
            { "Design Issues", new() { GetSelectStarQuery(), GetNPlusOneQuery() } },
            { "Type Conversion", new() { GetImplicitConversionQuery() } },
            { "Complex Joins", new() { GetComplexJoinQuery() } }
        };
}
