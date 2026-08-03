[Benchmark]
[Benchmark(MinTime = 100, MaxTime = 1000)]
[MemoryDiagnoser]
public class SlowQueryEntryBenchmarks
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
    [Params(10, 100, 1000)]
    public void BenchmarkMethod2(int inputSize)
    {
        // benchmark method 2
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // benchmark method 3
    }
}
