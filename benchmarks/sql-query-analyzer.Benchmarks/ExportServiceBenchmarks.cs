using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using SqlQueryAnalyzer.Export;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Benchmarks
{
    /// <summary>
    /// Benchmarks for the <see cref="ExportService"/> public API.
    /// </summary>
    [MemoryDiagnoser]
    public class ExportServiceBenchmarks
    {
        private ExportService _exportService = null!;
        private QueryAnalysisResult _singleResult = null!;
        private string _tempFilePath = null!;
        private string _tempDirectory = null!;

        /// <summary>
        /// Size of the batch for the batch‑export benchmark.
        /// </summary>
        [Params(10, 100, 1000)]
        public int BatchSize { get; set; }

        /// <summary>
        /// Global setup – creates a logger, the service instance and test data.
        /// </summary>
        [GlobalSetup]
        public void GlobalSetup()
        {
            // Use a null logger to avoid I/O overhead during benchmarking.
            var logger = NullLogger<ExportService>.Instance;
            _exportService = new ExportService(logger);

            // Create a minimal analysis result. The real type is defined elsewhere
            // in the solution; we rely on its default constructor and let the
            // formatters handle any null/default values.
            _singleResult = new QueryAnalysisResult();

            // Temporary paths – BenchmarkDotNet runs in an isolated folder,
            // but we still use the system temp directory to avoid permission issues.
            _tempFilePath = Path.Combine(Path.GetTempPath(), "benchmark_export.json");
            _tempDirectory = Path.Combine(Path.GetTempPath(), "benchmark_export_dir");
            Directory.CreateDirectory(_tempDirectory);
        }

        /// <summary>
        /// Benchmarks the single‑result export (ExportAsync).
        /// </summary>
        [Benchmark]
        public async Task ExportAsync()
        {
            await _exportService.ExportAsync(_singleResult, _tempFilePath, "json");
        }

        /// <summary>
        /// Benchmarks batch export (ExportBatchAsync) with varying batch sizes.
        /// </summary>
        [Benchmark]
        public async Task ExportBatchAsync()
        {
            var batch = new List<QueryAnalysisResult>(BatchSize);
            for (int i = 0; i < BatchSize; i++)
            {
                batch.Add(_singleResult);
            }

            await _exportService.ExportBatchAsync(batch, _tempFilePath, "json");
        }

        /// <summary>
        /// Benchmarks exporting the same result to multiple formats simultaneously.
        /// </summary>
        [Benchmark]
        public async Task ExportMultipleFormatsAsync()
        {
            await _exportService.ExportMultipleFormatsAsync(
                _singleResult,
                _tempDirectory,
                "json",
                "csv",
                "xml");
        }

        /// <summary>
        /// Benchmarks the full export package creation (ExportWithReportAsync).
        /// </summary>
        [Benchmark]
        public async Task ExportWithReportAsync()
        {
            await _exportService.ExportWithReportAsync(_singleResult, _tempDirectory);
        }
    }
}
