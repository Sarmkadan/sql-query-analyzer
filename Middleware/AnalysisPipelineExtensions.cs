using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Services;

namespace SqlQueryAnalyzer.Middleware
{
    /// <summary>
    /// Extension methods for <see cref="AnalysisPipeline"/> that provide convenient
    /// ways to compose and execute middleware in SQL query analysis scenarios.
    /// </summary>
    public static class AnalysisPipelineExtensions
    {
        /// <summary>
        /// Adds logging middleware to the pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline to configure.</param>
        /// <param name="logger">Optional logger instance; if null, creates a default.</param>
        /// <returns>The configured pipeline for method chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> is null.</exception>
        public static AnalysisPipeline UseLogging(this AnalysisPipeline pipeline, ILogger<LoggingMiddleware>? logger = null)
        {
            ArgumentNullException.ThrowIfNull(pipeline);

            pipeline.RegisterMiddleware(logger is null
                ? new LoggingMiddleware(NullLogger<LoggingMiddleware>.Instance)
                : new LoggingMiddleware(logger));

            return pipeline;
        }

        /// <summary>
        /// Adds validation middleware to the pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline to configure.</param>
        /// <param name="logger">Optional logger instance; if null, creates a default.</param>
        /// <returns>The configured pipeline for method chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> is null.</exception>
        public static AnalysisPipeline UseValidation(this AnalysisPipeline pipeline, ILogger<ValidationMiddleware>? logger = null)
        {
            ArgumentNullException.ThrowIfNull(pipeline);

            pipeline.RegisterMiddleware(logger is null
                ? new ValidationMiddleware(NullLogger<ValidationMiddleware>.Instance)
                : new ValidationMiddleware(logger));

            return pipeline;
        }

        /// <summary>
        /// Adds query normalization middleware to the pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline to configure.</param>
        /// <param name="logger">Optional logger instance; if null, creates a default.</param>
        /// <returns>The configured pipeline for method chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> is null.</exception>
        public static AnalysisPipeline UseNormalization(this AnalysisPipeline pipeline, ILogger<QueryNormalizationMiddleware>? logger = null)
        {
            ArgumentNullException.ThrowIfNull(pipeline);

            pipeline.RegisterMiddleware(logger is null
                ? new QueryNormalizationMiddleware(NullLogger<QueryNormalizationMiddleware>.Instance)
                : new QueryNormalizationMiddleware(logger));

            return pipeline;
        }

        /// <summary>
        /// Adds analysis middleware to the pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline to configure.</param>
        /// <param name="analyzer">The query analyzer service.</param>
        /// <param name="logger">Optional logger instance; if null, creates a default.</param>
        /// <returns>The configured pipeline for method chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> or <paramref name="analyzer"/> is null.</exception>
        public static AnalysisPipeline UseAnalysis(this AnalysisPipeline pipeline, IQueryAnalyzerService analyzer, ILogger<AnalysisMiddleware>? logger = null)
        {
            ArgumentNullException.ThrowIfNull(pipeline);
            ArgumentNullException.ThrowIfNull(analyzer);

            pipeline.RegisterMiddleware(logger is null
                ? new AnalysisMiddleware(analyzer, NullLogger<AnalysisMiddleware>.Instance)
                : new AnalysisMiddleware(analyzer, logger));

            return pipeline;
        }

        /// <summary>
        /// Adds optimization middleware to the pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline to configure.</param>
        /// <param name="logger">Optional logger instance; if null, creates a default.</param>
        /// <returns>The configured pipeline for method chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> is null.</exception>
        public static AnalysisPipeline UseOptimization(this AnalysisPipeline pipeline, ILogger<OptimizationMiddleware>? logger = null)
        {
            ArgumentNullException.ThrowIfNull(pipeline);

            pipeline.RegisterMiddleware(logger is null
                ? new OptimizationMiddleware(NullLogger<OptimizationMiddleware>.Instance)
                : new OptimizationMiddleware(logger));

            return pipeline;
        }

        /// <summary>
        /// Executes the pipeline with the given query string and returns the analysis result.
        /// </summary>
        /// <param name="pipeline">The configured pipeline.</param>
        /// <param name="query">The SQL query to analyze.</param>
        /// <returns>A task that completes with the analysis result.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> or <paramref name="query"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="query"/> is empty or whitespace.</exception>
        public static async Task<QueryAnalysisResult> AnalyzeQueryAsync(this AnalysisPipeline pipeline, string query)
        {
            ArgumentNullException.ThrowIfNull(pipeline);
            ArgumentException.ThrowIfNullOrWhiteSpace(query);

            var context = new global::SqlQueryAnalyzer.CLI.AnalysisContext { Query = query };
            await pipeline.ExecuteAsync(context).ConfigureAwait(false);
            return context.Result ?? throw new InvalidOperationException("Analysis failed to produce a result");
        }

        /// <summary>
        /// Executes the pipeline with the given queries in parallel and returns all results.
        /// </summary>
        /// <param name="pipeline">The configured pipeline.</param>
        /// <param name="queries">The SQL queries to analyze.</param>
        /// <returns>A task that completes with a read-only list of analysis results.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> or <paramref name="queries"/> is null.</exception>
        public static async Task<IReadOnlyList<QueryAnalysisResult>> AnalyzeQueriesAsync(this AnalysisPipeline pipeline, IReadOnlyList<string> queries)
        {
            ArgumentNullException.ThrowIfNull(pipeline);
            ArgumentNullException.ThrowIfNull(queries);

            var tasks = queries.Select(q => pipeline.AnalyzeQueryAsync(q)).ToList();
            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            return results;
        }

        /// <summary>
        /// Clears all middleware from the pipeline, allowing it to be reconfigured.
        /// </summary>
        /// <param name="pipeline">The pipeline to clear.</param>
        /// <returns>The cleared pipeline for method chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> is null.</exception>
        public static AnalysisPipeline ClearMiddleware(this AnalysisPipeline pipeline)
        {
            ArgumentNullException.ThrowIfNull(pipeline);
            pipeline.Clear();
            return pipeline;
        }

        /// <summary>
        /// Adds all standard middleware (logging, validation, normalization, analysis, optimization)
        /// to the pipeline in the recommended order for comprehensive query analysis.
        /// </summary>
        /// <param name="pipeline">The pipeline to configure.</param>
        /// <param name="analyzer">The query analyzer service required for analysis middleware.</param>
        /// <param name="logger">Optional logger instance to use for all middleware.</param>
        /// <returns>The fully configured pipeline for method chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> or <paramref name="analyzer"/> is null.</exception>
        public static AnalysisPipeline UseAllStandardMiddleware(
            this AnalysisPipeline pipeline,
            IQueryAnalyzerService analyzer,
            ILogger? logger = null)
        {
            ArgumentNullException.ThrowIfNull(pipeline);
            ArgumentNullException.ThrowIfNull(analyzer);

            return pipeline
                .UseLogging(logger as ILogger<LoggingMiddleware>)
                .UseValidation(logger as ILogger<ValidationMiddleware>)
                .UseNormalization(logger as ILogger<QueryNormalizationMiddleware>)
                .UseAnalysis(analyzer, logger as ILogger<AnalysisMiddleware>)
                .UseOptimization(logger as ILogger<OptimizationMiddleware>);
        }

        /// <summary>
        /// Gets the count of middleware registered in the pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline to inspect.</param>
        /// <returns>The number of middleware instances.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> is null.</exception>
        public static int GetMiddlewareCount(this AnalysisPipeline pipeline)
        {
            ArgumentNullException.ThrowIfNull(pipeline);
            return pipeline.MiddlewareCount;
        }

        /// <summary>
        /// Executes the pipeline with the given context and returns whether execution completed successfully.
        /// </summary>
        /// <param name="pipeline">The pipeline to execute.</param>
        /// <param name="context">The analysis context containing query and configuration.</param>
        /// <returns>True if pipeline completed successfully; false if execution was halted.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> or <paramref name="context"/> is null.</exception>
        public static async Task<bool> ExecuteWithSuccessCheckAsync(this AnalysisPipeline pipeline, global::SqlQueryAnalyzer.CLI.AnalysisContext context)
        {
            ArgumentNullException.ThrowIfNull(pipeline);
            ArgumentNullException.ThrowIfNull(context);

            await pipeline.ExecuteAsync(context).ConfigureAwait(false);
            return context.ShouldContinue;
        }
    }
}
