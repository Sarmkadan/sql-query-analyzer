[Benchmark]
[Benchmark(MinTime = 100, MaxTime = 1000)]
[MemoryDiagnoser]
public class QueryRewriteSuggestionBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Set up realistic test data here
    }

    [Benchmark]
    public void Benchmark_Method1()
    {
        // Test QueryRewriteSuggestion with input size 10
        var suggestion = new QueryRewriteSuggestion(10);
        // Perform some operation with suggestion
    }

    [Benchmark]
    public void Benchmark_Method2()
    {
        // Test QueryRewriteSuggestion with input size 100
        var suggestion = new QueryRewriteSuggestion(100);
        // Perform some operation with suggestion
    }

    [Benchmark]
    public void Benchmark_Method3()
    {
        // Test QueryRewriteSuggestion with input size 1000
        var suggestion = new QueryRewriteSuggestion(1000);
        // Perform some operation with suggestion
    }
}