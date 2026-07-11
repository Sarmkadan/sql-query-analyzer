# SqlPatternAnalyzer

The `SqlPatternAnalyzer` is a static utility class designed to analyze SQL query strings for common anti-patterns, performance issues, and structural characteristics. It provides methods to detect problematic query constructs, extract query components, and generate optimization recommendations, enabling developers to identify potential inefficiencies in SQL queries programmatically.

## API

### `public static bool DetectNPlusOnePattern`
Detects the N+1 query anti-pattern, where a query is executed inside a loop, leading to excessive database round trips.
- **Parameters**: None.
- **Returns**: `true` if the query exhibits characteristics of the N+1 pattern (e.g., repeated similar queries with varying parameters); otherwise, `false`.
- **Throws**: None.

### `public static HashSet<string> ExtractCteNames`
Extracts the names of Common Table Expressions (CTEs) from a SQL query.
- **Parameters**: None (operates on the query string context provided elsewhere).
- **Returns**: A `HashSet<string>` containing the names of all CTEs found in the query. Returns an empty set if no CTEs are present.
- **Throws**: None.

### `public static List<string> ExtractTablesFromQuery`
Identifies and extracts table names referenced in the FROM clause, JOIN clauses, or subqueries of a SQL query.
- **Parameters**: None.
- **Returns**: A `List<string>` of table names, including aliases if present. Returns an empty list if no tables are found.
- **Throws**: None.

### `public static bool HasMissingWhereClause`
Determines whether a query lacks a WHERE clause, which may indicate a full table scan.
- **Parameters**: None.
- **Returns**: `true` if the query has no WHERE clause; otherwise, `false`.
- **Throws**: None.

### `public static bool HasSelectStar`
Checks if the query uses `SELECT *`, which can lead to performance issues and unnecessary data retrieval.
- **Parameters**: None.
- **Returns**: `true` if the query contains `SELECT *`; otherwise, `false`.
- **Throws**: None.

### `public static bool HasLeadingWildcardLike`
Detects the use of leading wildcards in LIKE clauses (e.g., `LIKE '%value'`), which prevents index usage.
- **Parameters**: None.
- **Returns**: `true` if a leading wildcard is found in any LIKE clause; otherwise, `false`.
- **Throws**: None.

### `public static bool HasFunctionOnColumn`
Identifies the application of functions to columns in WHERE clauses (e.g., `WHERE UPPER(column) = 'VALUE'`), which can prevent index usage.
- **Parameters**: None.
- **Returns**: `true` if a function is applied to a column in a filter condition; otherwise, `false`.
- **Throws**: None.

### `public static bool HasImplicitJoin`
Detects implicit joins (e.g., `FROM table1, table2` without an explicit JOIN clause), which can lead to Cartesian products if not properly constrained.
- **Parameters**: None.
- **Returns**: `true` if an implicit join is detected; otherwise, `false`.
- **Throws**: None.

### `public static bool HasDistinctWithoutOrder`
Checks if a query uses `DISTINCT` without an `ORDER BY` clause, which may indicate unnecessary deduplication.
- **Parameters**: None.
- **Returns**: `true` if `DISTINCT` is present without `ORDER BY`; otherwise, `false`.
- **Throws**: None.

### `public static int CountOrConditions`
Counts the number of `OR` conditions in the WHERE clause, which can impact query performance and index usage.
- **Parameters**: None.
- **Returns**: The number of `OR` conditions. Returns `0` if no `OR` conditions are found.
- **Throws**: None.

### `public static bool HasSubquery`
Determines whether the query contains subqueries, which may complicate execution plans and performance.
- **Parameters**: None.
- **Returns**: `true` if subqueries are present; otherwise, `false`.
- **Throws**: None.

### `public static int CountUnion`
Counts the number of `UNION` or `UNION ALL` operations in the query, which can indicate complex query structures.
- **Parameters**: None.
- **Returns**: The number of `UNION` operations. Returns `0` if no `UNION` operations are found.
- **Throws**: None.

### `public static List<string> ExtractJoinConditions`
Extracts all JOIN conditions from the query, including the join type (e.g., INNER, LEFT) and the joined tables.
- **Parameters**: None.
- **Returns**: A `List<string>` of JOIN conditions. Returns an empty list if no JOIN clauses are found.
- **Throws**: None.

### `public static string ExtractWhereClause`
Extracts the WHERE clause from the query, if present.
- **Parameters**: None.
- **Returns**: The WHERE clause as a string, or `null` if no WHERE clause exists.
- **Throws**: None.

### `public static int CountCaseStatements`
Counts the number of `CASE` statements in the query, which may indicate complex conditional logic.
- **Parameters**: None.
- **Returns**: The number of `CASE` statements. Returns `0` if no `CASE` statements are found.
- **Throws**: None.

### `public static bool HasAggregateFunction`
Checks if the query contains aggregate functions (e.g., `COUNT`, `SUM`, `AVG`).
- **Parameters**: None.
- **Returns**: `true` if aggregate functions are present; otherwise, `false`.
- **Throws**: None.

### `public static bool HasWindowFunction`
Determines whether the query uses window functions (e.g., `ROW_NUMBER()`, `RANK()`).
- **Parameters**: None.
- **Returns**: `true` if window functions are present; otherwise, `false`.
- **Throws**: None.

### `public static double CalculateReadabilityScore`
Calculates a readability score for the query based on factors such as length, complexity, and structural clarity.
- **Parameters**: None.
- **Returns**: A `double` representing the readability score, where higher values indicate better readability.
- **Throws**: None.

### `public static int CountParentheses`
Counts the number of parentheses pairs in the query, which can indicate nested subqueries or complex expressions.
- **Parameters**: None.
- **Returns**: The number of parentheses pairs. Returns `0` if no parentheses are found.
- **Throws**: None.

### `public static List<string> GenerateOptimizationRecommendations`
Generates a list of optimization recommendations based on the detected patterns and query structure.
- **Parameters**: None.
- **Returns**: A `List<string>` of recommendations. Returns an empty list if no optimizations are identified.
- **Throws**: None.

## Usage

### Example 1: Analyzing a Query for Anti-Patterns
