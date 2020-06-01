# SqlPatternAnalyzerBenchmarks

The `SqlPatternAnalyzerBenchmarks` class is a specialized suite designed for measuring the performance of the `SqlPatternAnalyzer` component within the SQL Query Analyzer project. It provides isolated benchmark methods that quantify the execution time and memory allocation of core analysis tasks, including N+1 anti-pattern detection, table name extraction, query recommendation generation, and various structural query parsing operations such as readability scoring and condition counting. This suite is essential for tracking performance regressions in the pattern detection pipeline.

## API

### Setup
`public void Setup`
Prepares the environment and initializes necessary query data before benchmark execution. This method is typically called by the BenchmarkDotNet framework before each iteration set.

### DetectNPlusOneRepeated
`public bool DetectNPlusOneRepeated`
Benchmarks the detection of repeated N+1 query patterns in SQL statements. Returns `true` if the pattern is detected, `false` otherwise.

### DetectNPlusOneDiverse
`public bool DetectNPlusOneDiverse`
Benchmarks the detection of diverse N+1 query patterns (e.g., heterogeneous resource access). Returns `true` if the pattern is detected, `false` otherwise.

### ExtractTablesProblematic
`public List<string> ExtractTablesProblematic`
Benchmarks the table extraction logic when applied to complex or non-standard SQL queries. Returns a list of extracted table names.

### ExtractTablesNested
`public List<string> ExtractTablesNested`
Benchmarks the table extraction logic when applied to queries containing nested subqueries. Returns a list of extracted table names.

### RecommendationsClean
`public List<string> RecommendationsClean`
Benchmarks the generation of performance optimization recommendations for well-formed, clean SQL queries. Returns a list of recommendation strings.

### RecommendationsProblematic
`public List<string> RecommendationsProblematic`
Benchmarks the generation of performance optimization recommendations for queries containing identified anti-patterns. Returns a list of recommendation strings.

### ReadabilityScoreProblematic
`public double ReadabilityScoreProblematic`
Benchmarks the calculation of a readability score for complex or poorly formatted SQL queries. Returns a double representing the readability metric.

### CountParenthesesNested
`public int CountParenthesesNested`
Benchmarks the parsing and counting of nested parentheses within SQL expressions. Returns the total count of nested parentheses structures.

### HasFunctionOnColumn
`public bool HasFunctionOnColumn`
Benchmarks the detection of function applications on column expressions (e.g., `WHERE YEAR(date_column) = 2026`). Returns `true` if a function is detected on a column, `false` otherwise.

### CountOrConditions
`public int CountOrConditions`
Benchmarks the logic for parsing and counting the number of `OR` conditions within a `WHERE` or `JOIN` clause. Returns the total number of `OR` conditions found.

## Usage

### Example 1: Individual Method Invocation
This example demonstrates how the methods can be invoked directly, typically within a performance test or diagnostic utility.

```csharp
using SqlQueryAnalyzer.Benchmarks;

var benchmarks = new SqlPatternAnalyzerBenchmarks();
benchmarks.Setup();

// Measure performance of table extraction on complex SQL
var extractedTables = benchmarks.ExtractTablesProblematic();
Console.WriteLine($"Extracted {extractedTables.Count} tables.");
```

### Example 2: Integration with BenchmarkDotNet
This class is designed primarily for use with the BenchmarkDotNet library to automate performance tracking.

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SqlQueryAnalyzer.Benchmarks;

[MemoryDiagnoser]
public class PatternAnalysisRunner
{
    private SqlPatternAnalyzerBenchmarks _benchmarks = new();

    [GlobalSetup]
    public void Setup() => _benchmarks.Setup();

    [Benchmark]
    public bool RunNPlusOneDetection() => _benchmarks.DetectNPlusOneRepeated();
}

// In a console app:
// var summary = BenchmarkRunner.Run<PatternAnalysisRunner>();
```

## Notes

- **Input Data**: The benchmarks rely on pre-configured SQL query strings populated during the `Setup` phase. Performance results may vary significantly based on the complexity, size, and structure of these predefined queries.
- **Thread Safety**: While the methods themselves are intended to be stateless and thread-safe, the `Setup` method is not intended for concurrent execution. These benchmarks should be executed within the single-threaded context provided by the BenchmarkDotNet runner.
- **Edge Cases**: When implementing or extending these benchmarks, consider that empty or malformed SQL input may lead to unexpected parsing behavior; input validation should be robustly handled within the `SqlPatternAnalyzer` logic being tested.
