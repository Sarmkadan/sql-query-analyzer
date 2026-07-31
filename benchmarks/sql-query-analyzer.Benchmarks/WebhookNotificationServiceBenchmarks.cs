[Benchmark]
public class WebhookNotificationServiceBenchmarks
{
    [MemoryDiagnoser]
    public WebhookNotificationServiceBenchmarks()
    {
    }

    [Benchmark]
    public void BenchmarkMethod1()
    {
        // Setup test data
        var testService = new WebhookNotificationService();
        var testData = new TestData();
        // Benchmark code here
    }

    [Benchmark]
    public void BenchmarkMethod2([Params(10, 100, 1000)])
    {
        // Setup test data
        var testService = new WebhookNotificationService();
        var testData = new TestData();
        // Benchmark code here
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // Setup test data
        var testService = new WebhookNotificationService();
        var testData = new TestData();
        // Benchmark code here
    }
}