using BenchmarkDotNet.Attributes;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Benchmarks;

[MemoryDiagnoser]
public class StringExtensionsBenchmarks
{
    private const string Input = "This is a long string that needs to be truncated for testing purposes.";

    [Benchmark]
    public string Truncate()
    {
        return Input.Truncate(20);
    }

    [Benchmark]
    public string NormalizeWhitespace()
    {
        return "  multiple   spaces   here  ".NormalizeSqlWhitespace();
    }
}
