[Benchmark]
[MemoryDiagnoser]
public class ProfilerOptionsBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // setup test data
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void BenchmarkMethod1()
    {
        // benchmark method 1
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void BenchmarkMethod2()
    {
        // benchmark method 2
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void BenchmarkMethod3()
    {
        // benchmark method 3
    }
}
