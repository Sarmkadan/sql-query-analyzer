[Benchmark]
[MemoryDiagnoser]
public class PerformanceIssueBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // setup test data
    }

    [Benchmark]
    public void BenchmarkMethod1()
    {
        // benchmark method 1
    }

    [Benchmark]
    public void BenchmarkMethod2([Params(10)])
    {
        // benchmark method 2 with input size 10
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // benchmark method 3
    }
}
