using BenchmarkDotNet.Attributes;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Benchmarks;

[MemoryDiagnoser]
public class PerformanceMetricsCalculatorBenchmarks
{
    private readonly DatabaseQuery _query = new() { QueryText = "SELECT * FROM Orders" };

    [Benchmark]
    public int CalculateComplexity()
    {
        return PerformanceMetricsCalculator.CalculateComplexityScore(_query);
    }
}
