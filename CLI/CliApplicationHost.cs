#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Services;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Middleware;
using SqlQueryAnalyzer.Formatters;

namespace SqlQueryAnalyzer.CLI;

/// <summary>
/// Orchestrates the CLI application lifecycle.
/// Handles argument parsing, service initialization, and execution flow.
/// Separates CLI concerns from core analyzer logic.
/// </summary>
public class CliApplicationHost
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CliApplicationHost> _logger;
    private readonly IQueryAnalyzerService _analyzer;
    private readonly AnalysisPipeline _pipeline;

    /// <summary>
    /// Gets or sets the SQL query being analyzed.
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the command-line arguments.
    /// </summary>
    public CommandLineArguments Arguments { get; set; } = new();

    /// <summary>
    /// Gets or sets the analysis result.
    /// </summary>
    public QueryAnalysisResult? Result { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether analysis should continue.
    /// </summary>
    public bool ShouldContinue { get; set; } = true;

    /// <summary>
    /// Gets or sets metadata dictionary.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    public override string ToString() => $"CliApplicationHost {{ Query = {Query}, Arguments = {Arguments}, Result = {Result}, ShouldContinue = {ShouldContinue}, Metadata = {Metadata} }}";

    public CliApplicationHost(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<CliApplicationHost>>();
        _analyzer = serviceProvider.GetRequiredService<IQueryAnalyzerService>();
        _pipeline = serviceProvider.GetRequiredService<AnalysisPipeline>();
    }

    /// <summary>
    /// Main execution entry point for CLI.
    /// Validates arguments, initializes pipeline, and coordinates analysis.
    /// </summary>
    public async Task<int> RunAsync(CommandLineArguments args)
    {
        try
        {
            if (args.ShowHelp)
            {
                CommandLineParser.PrintHelp();
                return 0;
            }

            if (args.ShowVersion)
            {
                CommandLineParser.PrintVersion();
                return 0;
            }

            args.Validate();
            _logger.LogInformation("SQL Query Analyzer initialized with CLI arguments");

            // Read query from file or use provided query
            string queryText = await GetQueryTextAsync(args);
            if (string.IsNullOrEmpty(queryText))
            {
                _logger.LogError("No query provided");
                return 1;
            }

            // Run through pipeline
            var context = new AnalysisContext { Query = queryText, Arguments = args };
            await _pipeline.ExecuteAsync(context);

            // Format and output results
            if (context.Result != null)
            {
                await OutputResultsAsync(context.Result, args);
            }

            _logger.LogInformation("Analysis completed successfully");
            return 0;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError($"Invalid arguments: {ex.Message}");
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Use --help for usage information");
            return 2;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in CLI");
            Console.WriteLine($"Error: {ex.Message}");
            if (args.Verbose)
                Console.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    /// <summary>
    /// Loads query from either direct input or file.
    /// Applies basic validation and normalization.
    /// </summary>
    private async Task<string> GetQueryTextAsync(CommandLineArguments args)
    {
        if (!string.IsNullOrEmpty(args.Query))
        {
            return args.Query;
        }

        if (!string.IsNullOrEmpty(args.QueryFile))
        {
            if (!File.Exists(args.QueryFile))
            {
                throw new FileNotFoundException($"Query file not found: {args.QueryFile}");
            }

            return await File.ReadAllTextAsync(args.QueryFile);
        }

        return string.Empty;
    }

    /// <summary>
    /// Formats results according to specified output format and writes to file or console.
    /// </summary>
    private async Task OutputResultsAsync(QueryAnalysisResult result, CommandLineArguments args)
    {
        var formatter = GetFormatter(args.OutputFormat ?? "json");
        var output = formatter.Format(result);

        if (!string.IsNullOrEmpty(args.OutputPath))
        {
            await File.WriteAllTextAsync(args.OutputPath, output);
            _logger.LogInformation($"Results written to {args.OutputPath}");
        }
        else
        {
            Console.WriteLine(output);
        }

        // Generate HTML report if requested
        if (args.GenerateReport)
        {
            var reportPath = Path.ChangeExtension(args.OutputPath ?? "analysis_report.html", ".html");
            var reportContent = GenerateHtmlReport(result);
            await File.WriteAllTextAsync(reportPath, reportContent);
            _logger.LogInformation($"Report generated: {reportPath}");
        }
    }

    /// <summary>
    /// Returns appropriate formatter based on output format specification.
    /// </summary>
    private IResultFormatter GetFormatter(string format) =>
        format.ToLower() switch
        {
            "json" => _serviceProvider.GetRequiredService<JsonResultFormatter>(),
            "csv" => _serviceProvider.GetRequiredService<CsvResultFormatter>(),
            "xml" => _serviceProvider.GetRequiredService<XmlResultFormatter>(),
            "html" => _serviceProvider.GetRequiredService<HtmlResultFormatter>(),
            "text" => _serviceProvider.GetRequiredService<TextResultFormatter>(),
            _ => _serviceProvider.GetRequiredService<JsonResultFormatter>()
        };

    /// <summary>
    /// Creates a basic HTML report structure.
    /// Can be extended with styling and charts.
    /// </summary>
    private string GenerateHtmlReport(QueryAnalysisResult result)
    {
        var issuesSummary = result.Issues.GroupBy(i => i.Severity)
            .ToDictionary(g => g.Key, g => g.Count());

        return $@"
<!DOCTYPE html>
<html>
<head>
    <title>SQL Query Analysis Report</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ background: #f5f5f5; padding: 15px; border-radius: 4px; }}
        .score {{ font-size: 24px; font-weight: bold; color: #2196F3; }}
        .issues {{ margin-top: 20px; }}
        .issue {{ padding: 10px; margin: 5px 0; border-left: 4px solid #FF9800; background: #FFF3E0; }}
        .suggestion {{ padding: 10px; margin: 5px 0; border-left: 4px solid #4CAF50; background: #F1F8E9; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>SQL Query Analysis Report</h1>
        <div class='score'>Performance Score: {result.PerformanceScore:F1}/100</div>
        <p>Analyzed at: {result.AnalyzedAt:yyyy-MM-dd HH:mm:ss}</p>
        <p>Complexity: {result.Complexity}</p>
    </div>

    <div class='issues'>
        <h2>Issues Found</h2>
        <p>Critical: {issuesSummary.GetValueOrDefault(Constants.IssueSeverity.Critical, 0)}</p>
        <p>Warnings: {issuesSummary.GetValueOrDefault(Constants.IssueSeverity.Warning, 0)}</p>
        <p>Info: {issuesSummary.GetValueOrDefault(Constants.IssueSeverity.Info, 0)}</p>
    </div>
</body>
</html>";
    }
}

/// <summary>
/// Context passed through the analysis pipeline.
/// Carries query, arguments, and results between pipeline stages.
/// </summary>
public class AnalysisContext
{
    public string Query { get; set; } = string.Empty;
    public CommandLineArguments Arguments { get; set; } = new();
    public QueryAnalysisResult? Result { get; set; }
    public bool ShouldContinue { get; set; } = true;
    public Dictionary<string, object> Metadata { get; set; } = new();
}
