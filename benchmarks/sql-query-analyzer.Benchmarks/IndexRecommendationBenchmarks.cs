[MemoryDiagnoser]
public class IndexRecommendationBenchmarks
{
    [Benchmark]
    public void BenchmarkMethod1()
    {
        // setup test data
        var testData = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            testData.Add("testData" + i);
        }
        // benchmark code
        var sw = Stopwatch.StartNew();
        IndexRecommendation.BenchmarkMethod1(testData);
        sw.Stop();
        Console.WriteLine("Execution time: " + sw.ElapsedMilliseconds);
    }

    [Benchmark]
    public void BenchmarkMethod2([Params(10)] int inputSize)
    {
        // setup test data
        var testData = new List<string>();
        for (int i = 0; i < inputSize; i++)
        {
            testData.Add("testData" + i);
        }
        // benchmark code
        var sw = Stopwatch.StartNew();
        IndexRecommendation.BenchmarkMethod2(testData);
        sw.Stop();
        Console.WriteLine("Execution time: " + sw.ElapsedMilliseconds);
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // setup test data
        var testData = new Dictionary<string, string>();
        testData.Add("key1", "value1");
        testData.Add("key2", "value2");
        // benchmark code
        var sw = Stopwatch.StartNew();
        IndexRecommendation.BenchmarkMethod3(testData);
        sw.Stop();
        Console.WriteLine("Execution time: " + sw.ElapsedMilliseconds);
    }
}