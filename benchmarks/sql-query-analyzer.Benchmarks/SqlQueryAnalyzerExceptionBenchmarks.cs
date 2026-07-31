[Benchmark]
[Benchmark(MinTime = 100, MaxTime = 5000)]
[MemoryDiagnoser]
public class SqlQueryAnalyzerExceptionBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Setup test data here
    }

    [Benchmark]
    public void BenchmarkMethod1()
    {
        // Test method 1 here
        var result = SqlQueryAnalyzerException.Analyze();
        Assert.AreEqual(expectedResult, result);
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void BenchmarkMethod2(int inputSize)
    {
        // Test method 2 here
        var result = SqlQueryAnalyzerException.Analyze(inputSize);
        Assert.AreEqual(expectedResult, result);
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // Test method 3 here
        var result = SqlQueryAnalyzerException.Analyze();
        Assert.AreEqual(expectedResult, result);
    }
}
