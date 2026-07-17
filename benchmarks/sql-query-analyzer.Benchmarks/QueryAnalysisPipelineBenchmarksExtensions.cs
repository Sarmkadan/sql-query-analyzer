#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Benchmarks;

/// <summary>
/// Extension methods for <see cref="QueryAnalysisPipelineBenchmarks"/> that provide additional benchmarking scenarios
/// for query parsing, analysis, and pattern detection pipelines.
/// </summary>
/// <remarks>
/// All methods in this class are designed to be used in benchmark scenarios and return
/// either parsed queries or analysis results. Each method includes proper null checking and
/// follows the established patterns for query analysis in the SqlQueryAnalyzer project.
/// </remarks>
public sealed class QueryAnalysisPipelineBenchmarksExtensions
{
    /// <summary>
    /// Benchmarks parsing and analysis of a query with Common Table Expressions (CTEs).
    /// Tests recursive CTE handling, multiple CTE definitions, and CTE references.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <returns>The parsed query for further analysis.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">Thrown when query parsing fails.</exception>
    public static DatabaseQuery ParseWithCteQuery(this QueryAnalysisPipelineBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        const string cteQuery = @"
WITH SalesCTE AS (
    SELECT
        CustomerId,
        SUM(TotalAmount) AS TotalSales,
        COUNT(*) AS OrderCount
    FROM Orders
    WHERE OrderDate >= '2024-01-01'
    GROUP BY CustomerId
),
TopCustomersCTE AS (
    SELECT
        CustomerId,
        TotalSales,
        OrderCount,
        RANK() OVER (ORDER BY TotalSales DESC) AS SalesRank
    FROM SalesCTE
    WHERE TotalSales > 10000
)
SELECT
    c.CustomerName,
    tc.TotalSales,
    tc.OrderCount,
    tc.SalesRank
FROM Customers c
INNER JOIN TopCustomersCTE tc ON c.CustomerId = tc.CustomerId
WHERE tc.SalesRank <= 10
ORDER BY tc.TotalSales DESC;";

        var query = new DatabaseQuery { QueryText = cteQuery };
        query.Parse();
        return query;
    }

    /// <summary>
    /// Benchmarks parsing and analysis of a query with window functions.
    /// Tests OVER() clause parsing, PARTITION BY, ORDER BY, and window function detection.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <returns>The parsed query for further analysis.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">Thrown when query parsing fails.</exception>
    public static DatabaseQuery ParseWithWindowFunctionsQuery(this QueryAnalysisPipelineBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        const string windowFunctionsQuery = @"
SELECT
    p.ProductId,
    p.ProductName,
    p.CategoryId,
    SUM(p.UnitPrice) OVER (PARTITION BY p.CategoryId) AS CategoryTotalPrice,
    AVG(p.UnitPrice) OVER (PARTITION BY p.CategoryId ORDER BY p.ProductName ROWS BETWEEN 2 PRECEDING AND CURRENT ROW) AS CategoryAvgPrice,
    RANK() OVER (ORDER BY p.UnitPrice DESC) AS PriceRank,
    DENSE_RANK() OVER (PARTITION BY p.CategoryId ORDER BY p.UnitPrice DESC) AS CategoryPriceRank,
    ROW_NUMBER() OVER (ORDER BY p.ProductName) AS RowNum
FROM Products p
WHERE p.IsActive = 1
ORDER BY p.CategoryId, p.ProductName;";

        var query = new DatabaseQuery { QueryText = windowFunctionsQuery };
        query.Parse();
        return query;
    }

    /// <summary>
    /// Benchmarks parsing and hashing of a parameterized query.
    /// Tests parameter detection, normalization, and consistent hashing.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <returns>The generated hash for the parameterized query.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">Thrown when query parsing fails.</exception>
    public static string HashParameterizedQuery(this QueryAnalysisPipelineBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        const string parameterizedQuery = @"
SELECT
    o.OrderId,
    o.OrderDate,
    c.CustomerName,
    p.ProductName,
    od.Quantity,
    od.UnitPrice
FROM Orders o
INNER JOIN Customers c ON o.CustomerId = c.CustomerId
INNER JOIN OrderDetails od ON o.OrderId = od.OrderId
INNER JOIN Products p ON od.ProductId = p.ProductId
WHERE o.OrderDate >= @StartDate
AND c.Country = @Country
AND p.CategoryId = @CategoryId
ORDER BY o.OrderDate DESC
OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY;";

        var query = new DatabaseQuery { QueryText = parameterizedQuery };
        query.Parse();
        return query.GenerateHash();
    }

    /// <summary>
    /// Benchmarks extraction of join conditions from a query with multiple join types.
    /// Tests INNER JOIN, LEFT JOIN, RIGHT JOIN, and FULL OUTER JOIN parsing.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <returns>Read-only list of extracted join conditions.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> ExtractAllJoinConditions(this QueryAnalysisPipelineBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        const string multiJoinQuery = @"
SELECT
    a.ArticleId,
    a.Title,
    c.CategoryName,
    au.AuthorName,
    p.PublisherName,
    COUNT(l.ArticleId) AS LinkCount
FROM Articles a
INNER JOIN Categories c ON a.CategoryId = c.CategoryId
LEFT JOIN ArticleAuthors aa ON a.ArticleId = aa.ArticleId
LEFT JOIN Authors au ON aa.AuthorId = au.AuthorId
RIGHT JOIN Publishers p ON a.PublisherId = p.PublisherId
FULL OUTER JOIN ArticleLinks l ON a.ArticleId = l.ArticleId
WHERE a.PublishDate >= '2024-01-01'
GROUP BY a.ArticleId, a.Title, c.CategoryName, au.AuthorName, p.PublisherName
HAVING COUNT(l.ArticleId) > 5
ORDER BY LinkCount DESC;";

        return SqlPatternAnalyzer.ExtractJoinConditions(multiJoinQuery).AsReadOnly();
    }

    /// <summary>
    /// Benchmarks parsing and analysis of a query with subqueries in SELECT and WHERE clauses.
    /// Tests correlated subquery detection, EXISTS/NOT EXISTS, and IN clause parsing.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <returns>The parsed query for further analysis.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">Thrown when query parsing fails.</exception>
    public static DatabaseQuery ParseWithSubqueriesQuery(this QueryAnalysisPipelineBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        const string subqueriesQuery = @"
SELECT
    p.ProductId,
    p.ProductName,
    p.UnitPrice,
    (
        SELECT COUNT(*)
        FROM OrderDetails od
        WHERE od.ProductId = p.ProductId
    ) AS OrderCount,
    (
        SELECT AVG(od.Quantity)
        FROM OrderDetails od
        WHERE od.ProductId = p.ProductId
    ) AS AvgQuantity
FROM Products p
WHERE EXISTS (
    SELECT 1
    FROM OrderDetails od
    WHERE od.ProductId = p.ProductId
    AND od.Quantity > 10
)
AND p.ProductId IN (
    SELECT ProductId
    FROM OrderDetails
    WHERE Quantity * UnitPrice > 100
)
ORDER BY (SELECT SUM(od.Quantity) FROM OrderDetails od WHERE od.ProductId = p.ProductId) DESC;";

        var query = new DatabaseQuery { QueryText = subqueriesQuery };
        query.Parse();
        return query;
    }

    /// <summary>
    /// Benchmarks parsing and analysis of a query with CASE expressions and conditional logic.
    /// Tests CASE WHEN THEN ELSE END parsing, multiple conditions, and nested CASE expressions.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <returns>The parsed query for further analysis.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">Thrown when query parsing fails.</exception>
    public static DatabaseQuery ParseWithCaseExpressionsQuery(this QueryAnalysisPipelineBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        const string caseExpressionsQuery = @"
SELECT
    o.OrderId,
    o.OrderDate,
    CASE
        WHEN o.TotalAmount > 1000 THEN 'Premium'
        WHEN o.TotalAmount > 500 THEN 'Standard'
        WHEN o.TotalAmount > 100 THEN 'Basic'
        ELSE 'LowValue'
    END AS OrderCategory,
    CASE
        WHEN EXISTS (
            SELECT 1
            FROM OrderDetails od
            WHERE od.OrderId = o.OrderId
            AND od.Quantity > 20
        ) THEN 'BulkOrder'
        ELSE 'RegularOrder'
    END AS OrderType,
    SUM(
        CASE
            WHEN od.Quantity > 10 THEN od.Quantity * od.UnitPrice * 0.9
            ELSE od.Quantity * od.UnitPrice
        END
    ) AS DiscountedTotal
FROM Orders o
INNER JOIN OrderDetails od ON o.OrderId = od.OrderId
WHERE o.OrderDate >= '2024-01-01'
GROUP BY o.OrderId, o.OrderDate
ORDER BY DiscountedTotal DESC;";

        var query = new DatabaseQuery { QueryText = caseExpressionsQuery };
        query.Parse();
        return query;
    }

    /// <summary>
    /// Benchmarks parsing and hashing of a query with date/time functions and calculations.
    /// Tests date function detection, arithmetic operations, and consistent hashing.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <returns>The generated hash for the date/time query.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">Thrown when query parsing fails.</exception>
    public static string HashDateTimeFunctionsQuery(this QueryAnalysisPipelineBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        const string dateTimeQuery = @"
SELECT
    o.OrderId,
    o.OrderDate,
    DATEDIFF(day, o.OrderDate, GETDATE()) AS DaysSinceOrder,
    DATEADD(month, 3, o.OrderDate) AS EstimatedDeliveryDate,
    YEAR(o.OrderDate) AS OrderYear,
    MONTH(o.OrderDate) AS OrderMonth,
    DAY(o.OrderDate) AS OrderDay,
    DATEPART(weekday, o.OrderDate) AS OrderWeekday,
    o.TotalAmount * 1.08 AS TotalWithTax,
    o.TotalAmount * 1.08 * 0.9 AS TotalWithDiscountAndTax
FROM Orders o
WHERE o.OrderDate BETWEEN DATEADD(year, -1, GETDATE()) AND GETDATE()
ORDER BY DaysSinceOrder DESC;";

        var query = new DatabaseQuery { QueryText = dateTimeQuery };
        query.Parse();
        return query.GenerateHash();
    }

    /// <summary>
    /// Benchmarks extraction of join conditions and returns them as a formatted string.
    /// Useful for generating documentation or analysis reports from benchmark results.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <param name="queryText">The SQL query text to analyze.</param>
    /// <returns>Formatted string representation of join conditions.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> or <paramref name="queryText"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="queryText"/> is empty or whitespace.</exception>
    public static string FormatJoinConditions(this QueryAnalysisPipelineBenchmarks benchmarks, string queryText)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);
        ArgumentException.ThrowIfNullOrEmpty(queryText, nameof(queryText));

        var joinConditions = SqlPatternAnalyzer.ExtractJoinConditions(queryText);
        if (joinConditions.Count == 0)
        {
            return "No join conditions found.";
        }

        return string.Join("\n", joinConditions.Select((condition, index) =>
            $"{index + 1}. {condition}"));
    }
}