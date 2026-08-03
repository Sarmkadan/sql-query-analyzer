[Benchmark]
[Benchmark(MinTime = 100, MaxTime = 1000)]
[MemoryDiagnoser]
public class IndexSuggestionBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Initialize test data
    }

    [Benchmark]
    public void BenchmarkMethod1()
    {
        // Test IndexSuggestion with small input size
        var result = IndexSuggestion.Method1(10);
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void BenchmarkMethod2()
    {
        // Test IndexSuggestion with varying input sizes
        for (int i = 0; i < 3; i++)
        {
            var result = IndexSuggestion.Method2(10 * (i + 1));
        }
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // Test IndexSuggestion with large input size
        var result = IndexSuggestion.Method3(1000);
    }
}
