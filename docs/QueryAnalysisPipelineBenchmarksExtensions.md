# QueryAnalysisPipelineBenchmarksExtensions

Provides pre-configured query instances and helper strings used as standardized inputs for benchmarking the SQL query analysis pipeline. These static properties supply representative SQL constructs—including CTEs, window functions, subqueries, CASE expressions, parameterized queries, and date-time functions—enabling consistent performance measurement across analysis stages such as parsing, join extraction, and query hashing.

## API

### `ParseWithCteQuery`
**Type:** `DatabaseQuery`  
**Purpose:** Returns a `DatabaseQuery` instance containing a SQL query that uses a Common Table Expression (CTE). Used to benchmark parsing and analysis of CTE structures.  
**Returns:** A non-null `DatabaseQuery` representing a query with at least one CTE definition.  
**Throws:** Does not throw; the value is pre-initialized.

### `ParseWithWindowFunctionsQuery`
**Type:** `DatabaseQuery`  
**Purpose:** Returns a `DatabaseQuery` instance containing a SQL query that uses window functions (e.g., `ROW_NUMBER()`, `SUM() OVER`). Used to benchmark parsing and analysis of window function syntax.  
**Returns:** A non-null `DatabaseQuery` representing a query with window functions.  
**Throws:** Does not throw; the value is pre-initialized.

### `HashParameterizedQuery`
**Type:** `string`  
**Purpose:** Provides a parameterized SQL query string (containing placeholders such as `@param` or `?`) used to benchmark query hashing logic that normalizes literal values.  
**Returns:** A non-null, non-empty string containing a parameterized SQL statement.  
**Throws:** Does not throw; the value is a constant string.

### `ExtractAllJoinConditions`
**Type:** `IReadOnlyList<string>`  
**Purpose:** Supplies a read-only collection of join condition strings extracted from a representative multi-join query. Used to benchmark the join condition extraction and formatting pipeline.  
**Returns:** A non-null `IReadOnlyList<string>` containing one or more join condition expressions (e.g., `"t1.id = t2.id"`). The list is immutable.  
**Throws:** Does not throw; the collection is pre-populated.

### `ParseWithSubqueriesQuery`
**Type:** `DatabaseQuery`  
**Purpose:** Returns a `DatabaseQuery` instance containing a SQL query with nested subqueries in the `SELECT`, `FROM`, or `WHERE` clauses. Used to benchmark recursive parsing and analysis of subquery structures.  
**Returns:** A non-null `DatabaseQuery` representing a query with at least one subquery.  
**Throws:** Does not throw; the value is pre-initialized.

### `ParseWithCaseExpressionsQuery`
**Type:** `DatabaseQuery`  
**Purpose:** Returns a `DatabaseQuery` instance containing a SQL query with `CASE` expressions (both simple and searched forms). Used to benchmark parsing and analysis of conditional expressions.  
**Returns:** A non-null `DatabaseQuery` representing a query with `CASE` expressions.  
**Throws:** Does not throw; the value is pre-initialized.

### `HashDateTimeFunctionsQuery`
**Type:** `string`  
**Purpose:** Provides a SQL query string containing vendor-specific date-time functions (e.g., `GETDATE()`, `NOW()`, `DATEADD`) used to benchmark query hashing behavior with non-deterministic or dialect-specific functions.  
**Returns:** A non-null, non-empty string containing a SQL statement with date-time function calls.  
**Throws:** Does not throw; the value is a constant string.

### `FormatJoinConditions`
**Type:** `string`  
**Purpose:** Supplies a pre-formatted string representation of join conditions, used as an expected output baseline when benchmarking the join condition formatting logic.  
**Returns:** A non-null string containing formatted join conditions (e.g., `"INNER JOIN t2 ON t1.id = t2.id\nLEFT JOIN t3 ON t2.ref = t3.id"`).  
**Throws:** Does not throw; the value is a constant string.

## Usage

```csharp
using SqlQueryAnalyzer.Benchmarks;
using BenchmarkDotNet.Running;

var summary = BenchmarkRunner.Run<QueryAnalysisPipelineBenchmarks>();
```

```csharp
using SqlQueryAnalyzer.Benchmarks;
using SqlQueryAnalyzer.Core;

var cteQuery = QueryAnalysisPipelineBenchmarksExtensions.ParseWithCteQuery;
var joinConditions = QueryAnalysisPipelineBenchmarksExtensions.ExtractAllJoinConditions;

foreach (var condition in joinConditions)
{
    Console.WriteLine($"Join: {condition}");
}

var hashInput = QueryAnalysisPipelineBenchmarksExtensions.HashParameterizedQuery;
var hash = QueryHasher.ComputeHash(hashInput);
Console.WriteLine($"Parameterized query hash: {hash}");
```

## Notes

- All properties are thread-safe: they return either immutable reference types (`string`, `IReadOnlyList<string>`) or pre-constructed `DatabaseQuery` instances that are effectively immutable after initialization.
- The `DatabaseQuery` instances are shared across benchmark iterations; do not mutate their internal state (e.g., do not modify parsed AST nodes) as this would corrupt subsequent benchmark runs.
- `ExtractAllJoinConditions` returns a read-only list backed by an array; enumeration is allocation-free but the list itself must not be cast to a mutable type.
- The query strings (`HashParameterizedQuery`, `HashDateTimeFunctionsQuery`, `FormatJoinConditions`) are compile-time constants embedded in the assembly; they do not allocate per access.
- These members are intended exclusively for benchmark scenarios. They are not suitable for production query processing as they represent fixed test cases, not dynamic input handling.
