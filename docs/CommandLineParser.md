# CommandLineParser

`CommandLineParser` is a static class in `SqlQueryAnalyzer.CLI` that turns the raw `string[]` of console arguments into a populated `CommandLineArguments` object. It also prints the help text and version banner used by `CliApplicationHost`.

The parser is deliberately simple: it does no validation of values (apart from integer parsing), does not report unknown flags, and never throws on malformed input. Validation is left to `CommandLineArguments.Validate()` and the analysis pipeline.

## API

| Member | Signature | Description |
|--------|-----------|-------------|
| **Parse** | `public static CommandLineArguments Parse(string[] args)` | Walks the argument array left to right and returns a new `CommandLineArguments` with the recognised options set. Unrecognised flags and stray positional tokens are ignored. |
| **PrintHelp** | `public static void PrintHelp()` | Writes the full usage text (options grouped by category, plus examples) to `Console.Out`. |
| **PrintVersion** | `public static void PrintVersion()` | Writes the version, author and license lines to `Console.Out`. |

The private helpers `SetArgument`, `SetBooleanArgument` and `HandleShortFlag` contain the flag-to-property mapping. They are not part of the public surface.

### Supported syntax

| Form | Example | Handling |
|------|---------|----------|
| `--flag=value` | `--format=csv` | Split on the first `=`; leading dashes are trimmed from the name. |
| `--flag value` | `--query-file queries.sql` | Consumes the next token if it does not start with `--`. |
| `--flag` | `--report` | Boolean flag set to `true` when no value follows. |
| `-x` | `-v`, `-h` | Only exact two-character short flags. `-f` and `-o` take the next token as their value. |

Flag names are matched case-insensitively. Combined short flags such as `-vh` are not supported.

### Flag-to-property mapping

| Flag(s) | Property on `CommandLineArguments` | Kind |
|---------|-----------------------------------|------|
| `--query` | `Query` | value |
| `--query-file`, `--file`, `-f` | `QueryFile` | value |
| `--output`, `-o` | `OutputPath` | value |
| `--format` | `OutputFormat` | value |
| `--connection` | `DatabaseConnection` | value |
| `--config` | `ConfigFile` | value |
| `--threads` | `ThreadCount` | int value |
| `--severity` | `FilterBySeverity` | value |
| `--version <ver>` | `SqlServerVersion` | value |
| `--version` | `ShowVersion` | boolean |
| `--max-results` | `MaxResults` | int value |
| `--cache-path` | `CachePath` | value |
| `--slow-log`, `--slow-log-file` | `SlowLogFile` | value |
| `--slow-log-format` | `SlowLogFormat` | value |
| `--verbose`, `-v` | `Verbose` | boolean |
| `--report` | `GenerateReport` | boolean |
| `--batch` | `BatchMode` | boolean |
| `--execution-plan` | `ShowExecutionPlan` | boolean |
| `--dry-run` | `DryRun` | boolean |
| `--export-suggestions` | `ExportSuggestions` | boolean |
| `--cache` | `EnableCache` | boolean (can only set `true`) |
| `--help`, `-h` | `ShowHelp` | boolean |
| `--parse-slow-log` | `ParseSlowLog` | boolean |

## Behaviour and gotchas

* **`--version` is overloaded.** With a following value (`--version 2022`) it sets `SqlServerVersion`. Without one it sets `ShowVersion`.
* **Boolean flags can swallow a positional token.** `--report queries.sql` treats `queries.sql` as the value of `report`, where it is silently discarded. Put boolean flags after a value-taking option, or before another `--option`.
* **`-f` and `-o` do not advance the loop index.** Their value token is revisited on the next iteration. This is harmless unless the value starts with `-`.
* **Any token containing `=` that is not consumed as a value is parsed as `flag=value`.** A bare SQL fragment passed without a preceding `--query` is therefore misread.
* **Bad integers are ignored.** `--threads abc` leaves `ThreadCount` at its default (`Environment.ProcessorCount`).
* **Unknown options are ignored silently.** No error or warning is produced.
* **The cache cannot be disabled from the command line.** `--cache` only sets `EnableCache = true`, and there is no `--no-cache` counterpart.
* **`args` must not be `null`.** Passing `null` throws `NullReferenceException`. Passing an empty array returns a default `CommandLineArguments`.

## Usage

### Example – parsing a typical invocation

```csharp
using SqlQueryAnalyzer.CLI;

string[] argv =
{
    "--query-file", "queries.sql",
    "--format=csv",
    "--severity", "Critical",
    "--max-results", "10",
    "-v"
};

CommandLineArguments options = CommandLineParser.Parse(argv);

// options.QueryFile         == "queries.sql"
// options.OutputFormat      == "csv"
// options.FilterBySeverity  == "Critical"
// options.MaxResults        == 10
// options.Verbose           == true

if (options.ShowHelp)
{
    CommandLineParser.PrintHelp();
}
else if (options.ShowVersion)
{
    CommandLineParser.PrintVersion();
}
```

*The example mixes the space-separated and `=` forms, uses the `-v` short flag, and shows that `--max-results` is converted to an `int?`. Options that are not supplied keep the defaults declared on `CommandLineArguments` (for example, `EnableCache` is `true` and `SqlServerVersion` is `"2019"` unless overridden).*
