[MemoryDiagnoser]
public class IndexBenchmarks
{
    [Benchmark]
    public void BenchmarkMethod1()
    {
        // setup and test data
    }
    [Benchmark]
    public void BenchmarkMethod2([Params(10)] int inputSize)
    {
        // setup and test data
    }
    [Benchmark]
    public void BenchmarkMethod3()
    {
        // setup and test data
    }
}
