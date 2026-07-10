# QueryAnalysisPipelineBenchmarks

The `QueryAnalysisPipelineBenchmarks` class provides a suite of methods designed to measure and evaluate the performance of the core SQL parsing, hashing, and analysis components within the `sql-query-analyzer` project. This class is primarily intended for use within benchmarking frameworks to establish performance baselines, identify regressions in query processing logic, and optimize the execution speed of routine query analysis tasks across various query complexities.

## API

### ParseSimpleQuery
Executes the parsing logic for a simple SQL query.
*   **Parameters:** None.
*   **Return Value:** `void`.
*   **Exceptions:** Throws an exception if the underlying parsing engine encounters a structural error in the query.

### ParseComplexQuery
Executes the parsing logic for a complex SQL query.
*   **Parameters:** None.
*   **Return Value:** `void`.
*   **Exceptions:** Throws an exception if the parsing engine encounters a structural error in the query.

### ParseStoredProcQuery
Executes the parsing logic specifically for stored procedure queries.
*   **Parameters:** None.
*   **Return Value:** `void`.
*   **Exceptions:** Throws an exception if the parsing engine encounters a structural error in the query.

### HashSimpleQuery
Computes a cryptographic or structural hash for a simple SQL query.
*   **Parameters:** None.
*   **Return Value:** `string` representing the computed hash.
*   **Exceptions:** Throws an exception if the query hashing fails.

### HashComplexQuery
Computes a cryptographic or structural hash for a complex SQL query.
*   **Parameters:** None.
*   **Return Value:** `string` representing the computed hash.
*   **Exceptions:** Throws an exception if the query hashing fails.

### ExtractJoinConditions
Parses the query and extracts a list of all identified SQL join conditions.
*   **Parameters:** None.
*   **Return Value:** `List<string>` containing the extracted join conditions.
*   **Exceptions:** Throws an exception if the query cannot be processed to extract join conditions.

## Usage

### Example 1: Basic Benchmarking
```csharp
using sql_query_analyzer;

var benchmarks = new QueryAnalysisPipelineBenchmarks();

// Execute parsing benchmarks
benchmarks.ParseSimpleQuery();
benchmarks.ParseComplexQuery();

// Execute extraction benchmarks
var joinConditions = benchmarks.ExtractJoinConditions();
```

### Example 2: Integration with BenchmarkDotNet
```csharp
using BenchmarkDotNet.Attributes;
using sql_query_analyzer;

[MemoryDiagnoser]
public class SqlAnalysisBenchmark
{
    private readonly QueryAnalysisPipelineBenchmarks _benchmarks = new();

    [Benchmark]
    public string BenchmarkHashSimple()
    {
        return _benchmarks.HashSimpleQuery();
    }
}
```

## Notes

*   **Data Preparation:** The methods in this class rely on the internal state of the `QueryAnalysisPipelineBenchmarks` instance. Ensure that the appropriate query data is loaded or initialized within the instance (typically via a `[GlobalSetup]` method if using a benchmarking framework) before executing these methods.
*   **Thread Safety:** This class is not inherently thread-safe. Concurrent execution of these methods on a single instance may lead to unpredictable results or state corruption. It is recommended to use one instance per thread or serialize access to the instance.
*   **Exceptions:** If a query fails to parse or hash, an exception is thrown. In a benchmarking scenario, this will cause the benchmark run to terminate. Ensure that the queries provided for benchmarking are valid according to the parser's requirements.
