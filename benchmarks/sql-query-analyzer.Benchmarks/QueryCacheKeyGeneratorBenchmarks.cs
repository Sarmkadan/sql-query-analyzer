using BenchmarkDotNet.Attributes;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Benchmarks;

[MemoryDiagnoser]
public class QueryCacheKeyGeneratorBenchmarks
{
    private readonly QueryCacheKeyGenerator _generator = new();
    private const string Query = "SELECT * FROM Orders WHERE CustomerId = 1";

    [Benchmark]
    public string GenerateKey()
    {
        return _generator.GenerateQueryKey(Query);
    }
}
