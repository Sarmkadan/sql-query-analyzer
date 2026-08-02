[Benchmark]
[Benchmark(MinTime = 100, MaxTime = 1000)]
[MemoryDiagnoser]
public class HttpQueryAnalysisClientValidationBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // setup test data
    }

    [Benchmark]
    public void Benchmark_Method1()
    {
        // test method 1
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void Benchmark_Method2(int n)
    {
        // test method 2
    }

    [Benchmark]
    public void Benchmark_Method3()
    {
        // test method 3
    }
}