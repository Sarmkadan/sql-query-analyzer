# QueryNormalizerBenchmarks

The `QueryNormalizerBenchmarks` class provides a structured suite of performance benchmarks designed to evaluate the efficiency and resource consumption of the `QueryNormalizer` component within the `sql-query-analyzer` project. Utilizing the BenchmarkDotNet framework, this class measures the execution time and memory allocation of core normalization and metadata extraction tasks across varying query complexities, including scenarios with simple syntax, complex joins, and varying literal content.

## API

### `public void Setup()`
Performs global initialization for the benchmark suite, ensuring the `QueryNormalizer` instance is configured before any benchmark measurements begin.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Exceptions**: Throws `InvalidOperationException` if the normalization engine fails to initialize.

### `public string NormalizeSimple()`
Benchmarks the normalization process for a simple SQL query, primarily testing basic tokenization and structural simplification performance.
*   **Parameters**: None.
*   **Returns**: A `string` representing the normalized query.
*   **Exceptions**: Throws `ArgumentNullException` if the input query is null.

### `public string NormalizeComplex()`
Benchmarks the normalization process for a complex SQL query, testing the efficiency of handling multiple `JOIN` clauses, aggregations, and subqueries.
*   **Parameters**: None.
*   **Returns**: A `string` representing the normalized query.
*   **Exceptions**: Throws `ArgumentException` if the query syntax is unparseable.

### `public string NormalizeWithLiterals()`
Benchmarks the normalization process for queries containing various string literals, testing the engine's ability to identify and redact or handle literal values correctly.
*   **Parameters**: None.
*   **Returns**: A `string` representing the normalized query with literals handled.
*   **Exceptions**: Throws `ArgumentException` if the literal parsing logic encounters an invalid format.

### `public List<string> ExtractTableNamesComplex()`
Benchmarks the performance of extracting table names from a complex SQL query, simulating scenarios with deep `JOIN` hierarchies.
*   **Parameters**: None.
*   **Returns**: A `List<string>` containing the identified table names.
*   **Exceptions**: Throws `ArgumentException` if the input query structure prevents correct table name resolution.

### `public List<string> ExtractColumnNamesComplex()`
Benchmarks the performance of identifying and extracting column names from a complex SQL query.
*   **Parameters**: None.
*   **Returns**: A `List<string>` containing the identified column names.
*   **Exceptions**: Throws `ArgumentException` if the input query structure prevents correct column name resolution.

## Usage

### Running Benchmarks
To execute the defined benchmarks, use the `BenchmarkRunner` provided by BenchmarkDotNet:

```csharp
using BenchmarkDotNet.Running;
using SqlQueryAnalyzer.Benchmarks;

// Run all methods marked with [Benchmark] in QueryNormalizerBenchmarks
var summary = BenchmarkRunner.Run<QueryNormalizerBenchmarks>();
```

### Extending Benchmark Scenarios
To add a new benchmark for a specific SQL pattern, inherit from or add a method to the class decorated with the `[Benchmark]` attribute:

```csharp
[Benchmark]
public string NormalizeSpecialSyntax()
{
    // Define the specific SQL pattern
    string query = "SELECT * FROM Table WHERE Col = @Param";
    // Invoke the normalizer
    return _normalizer.Normalize(query);
}
```

## Notes

*   **Thread Safety**: This class is designed for use within the BenchmarkDotNet harness, which manages its own thread affinity. The underlying `QueryNormalizer` instance initialized in `Setup` may not be thread-safe; therefore, `QueryNormalizerBenchmarks` should not be used in multi-threaded contexts outside of controlled benchmark execution.
*   **Memory Diagnostics**: The class is decorated with `[MemoryDiagnoser]`, which incurs additional overhead. When evaluating results, account for the diagnostic tracking impact on measured execution times.
*   **Edge Cases**: Benchmarks include queries with unbalanced quotes, malformed `JOIN` syntax, and unconventional whitespace to ensure the normalizer handles realistic but malformed input without excessive performance degradation or crashes. Ensure the input constants within the benchmark class reflect the expected diversity of production queries.
