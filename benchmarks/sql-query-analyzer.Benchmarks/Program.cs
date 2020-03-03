// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Running;
using SqlQueryAnalyzer.Benchmarks;

BenchmarkRunner.Run(
[
    typeof(QueryNormalizerBenchmarks),
    typeof(SqlPatternAnalyzerBenchmarks),
    typeof(QueryAnalysisPipelineBenchmarks),
]);
