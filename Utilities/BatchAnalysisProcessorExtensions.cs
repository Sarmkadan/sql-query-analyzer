using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides extension methods for <see cref="BatchAnalysisProcessor"/>.
/// </summary>
public static class BatchAnalysisProcessorExtensions
{
    /// <summary>
    /// Configures the maximum degree of parallelism for the processor and returns the processor instance.
    /// </summary>
    /// <param name="processor">The processor instance.</param>
    /// <param name="maxParallel">The maximum number of concurrent tasks.</param>
    /// <returns>The <see cref="BatchAnalysisProcessor"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="processor"/> is null.</exception>
    public static BatchAnalysisProcessor WithMaxParallel(this BatchAnalysisProcessor processor, int maxParallel)
    {
        ArgumentNullException.ThrowIfNull(processor);
        processor.SetMaxParallel(maxParallel);
        return processor;
    }

    /// <summary>
    /// Analyzes a batch of queries in parallel.
    /// Returns a read-only list of analysis results in input order.
    /// </summary>
    /// <param name="processor">The processor instance.</param>
    /// <param name="queries">The collection of queries to analyze.</param>
    /// <param name="onProgress">Optional callback for progress reporting.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of analysis results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="processor"/> or <paramref name="queries"/> is null.</exception>
    public static async Task<IReadOnlyList<QueryAnalysisResult>> AnalyzeBatchAsync(
        this BatchAnalysisProcessor processor,
        IEnumerable<string> queries,
        Action<BatchProgress>? onProgress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(processor);
        ArgumentNullException.ThrowIfNull(queries);
        return await processor.AnalyzeBatchAsync(queries.ToArray(), onProgress, cancellationToken);
    }

    /// <summary>
    /// Analyzes queries from the specified file (one per line).
    /// Returns a read-only list of analysis results.
    /// </summary>
    /// <param name="processor">The processor instance.</param>
    /// <param name="file">The file information containing queries.</param>
    /// <param name="onProgress">Optional callback for progress reporting.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of analysis results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="processor"/> or <paramref name="file"/> is null.</exception>
    public static async Task<IReadOnlyList<QueryAnalysisResult>> AnalyzeBatchFromFileAsync(
        this BatchAnalysisProcessor processor,
        FileInfo file,
        Action<BatchProgress>? onProgress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(processor);
        ArgumentNullException.ThrowIfNull(file);
        return await processor.AnalyzeBatchFromFileAsync(file.FullName, onProgress, cancellationToken);
    }
}
