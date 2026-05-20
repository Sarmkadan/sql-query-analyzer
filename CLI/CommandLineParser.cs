#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;

namespace SqlQueryAnalyzer.CLI;

/// <summary>
/// Parses command-line arguments into CommandLineArguments.
/// Uses simple but robust parsing to handle common CLI patterns.
/// </summary>
public static class CommandLineParser
{
    /// <summary>
    /// Parses array of command-line arguments into structured object.
    /// Supports both --flag=value and --flag value formats.
    /// </summary>
    public static CommandLineArguments Parse(string[] args)
    {
        var arguments = new CommandLineArguments();

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            // Handle flag=value format
            if (arg.Contains('='))
            {
                var parts = arg.Split('=', 2);
                var flag = parts[0].TrimStart('-');
                var value = parts[1];
                SetArgument(arguments, flag, value);
            }
            // Handle flag value format
            else if (arg.StartsWith("--") && i + 1 < args.Length && !args[i + 1].StartsWith("--"))
            {
                var flag = arg.TrimStart('-');
                var value = args[++i];
                SetArgument(arguments, flag, value);
            }
            // Handle boolean flags
            else if (arg.StartsWith("--"))
            {
                var flag = arg.TrimStart('-');
                SetBooleanArgument(arguments, flag, true);
            }
            // Handle single-dash short flags
            else if (arg.StartsWith("-") && arg.Length == 2)
            {
                HandleShortFlag(arguments, arg[1], i, args);
            }
        }

        return arguments;
    }

    /// <summary>
    /// Displays help text with all supported options and examples.
    /// </summary>
    public static void PrintHelp()
    {
        Console.WriteLine(@"
SQL Query Analyzer v1.0.0
Advanced SQL query analysis for performance optimization

USAGE:
  sqlanalyzer [OPTIONS]

OPTIONS:
  Query Input:
    --query <sql>              SQL query to analyze
    --query-file <path>        Path to file containing SQL query
    -f, --file <path>         Alias for --query-file

  Output:
    -o, --output <path>        Output file path
    --format <type>            Output format: json|csv|xml|html|text (default: json)
    --report                   Generate detailed HTML report
    --export-suggestions       Export index suggestions to file

  Database:
    --connection <string>      Database connection string
    --version <ver>            SQL Server version: 2019|2022 (default: 2019)

  Analysis Options:
    --execution-plan           Include execution plan in output
    --severity <level>         Filter by severity: Critical|Warning|Info
    --max-results <n>          Limit results to N issues
    --parse-slow-log           Parse slow query log file
    --slow-log-file <path>     Path to slow query log file
    --slow-log-format <fmt>    Log format: mysql|postgresql|sqlserver (default: mysql)

  Performance:
    --batch                    Enable batch mode for multiple queries
    --threads <n>              Number of parallel threads (default: CPU count)
    --cache                    Enable result caching (default: true)
    --cache-path <path>        Custom cache directory

  Other:
    --config <path>            Load configuration from JSON file
    --dry-run                  Run without making changes (analysis only)
    -v, --verbose              Verbose output logging
    -h, --help                 Show this help message
    --version                  Show version information

EXAMPLES:
  # Analyze a single query
  sqlanalyzer --query ""SELECT * FROM Orders""

  # Analyze from file with report
  sqlanalyzer --query-file queries.sql --report --format json

  # Batch analysis with custom threading
  sqlanalyzer --batch --threads 4 --cache-path ./cache

  # Export with severity filtering
  sqlanalyzer --query-file query.sql --severity Critical --export-suggestions
");
    }

    /// <summary>
    /// Displays version information.
    /// </summary>
    public static void PrintVersion()
    {
        Console.WriteLine("SQL Query Analyzer v1.0.0");
        Console.WriteLine("Author: Vladyslav Zaiets");
        Console.WriteLine("License: MIT");
    }

    private static void SetArgument(CommandLineArguments args, string flag, string value)
    {
        switch (flag.ToLower())
        {
            case "query":
                args.Query = value;
                break;
            case "query-file" or "file" or "f":
                args.QueryFile = value;
                break;
            case "output" or "o":
                args.OutputPath = value;
                break;
            case "format":
                args.OutputFormat = value;
                break;
            case "connection":
                args.DatabaseConnection = value;
                break;
            case "config":
                args.ConfigFile = value;
                break;
            case "threads":
                if (int.TryParse(value, out var threads))
                    args.ThreadCount = threads;
                break;
            case "severity":
                args.FilterBySeverity = value;
                break;
            case "version":
                args.SqlServerVersion = value;
                break;
            case "max-results":
                if (int.TryParse(value, out var max))
                    args.MaxResults = max;
                break;
            case "cache-path":
                args.CachePath = value;
                break;
            case "slow-log" or "slow-log-file":
                args.SlowLogFile = value;
                break;
            case "slow-log-format":
                args.SlowLogFormat = value;
                break;
        }
    }

    private static void SetBooleanArgument(CommandLineArguments args, string flag, bool value)
    {
        switch (flag.ToLower())
        {
            case "verbose" or "v":
                args.Verbose = value;
                break;
            case "report":
                args.GenerateReport = value;
                break;
            case "batch":
                args.BatchMode = value;
                break;
            case "execution-plan":
                args.ShowExecutionPlan = value;
                break;
            case "dry-run":
                args.DryRun = value;
                break;
            case "export-suggestions":
                args.ExportSuggestions = value;
                break;
            case "cache":
                args.EnableCache = value;
                break;
            case "help" or "h":
                args.ShowHelp = value;
                break;
            case "version":
                args.ShowVersion = value;
                break;
            case "parse-slow-log":
                args.ParseSlowLog = value;
                break;
        }
    }

    private static void HandleShortFlag(CommandLineArguments args, char flag, int index, string[] allArgs)
    {
        switch (char.ToLower(flag))
        {
            case 'f':
                if (index + 1 < allArgs.Length)
                    args.QueryFile = allArgs[index + 1];
                break;
            case 'o':
                if (index + 1 < allArgs.Length)
                    args.OutputPath = allArgs[index + 1];
                break;
            case 'v':
                args.Verbose = true;
                break;
            case 'h':
                args.ShowHelp = true;
                break;
        }
    }
}
