# CliApplicationHostExtensions

The `CliApplicationHostExtensions` class provides a set of static extension methods designed to facilitate interaction with the application host within the SQL Query Analyzer command-line interface. These utilities enable the retrieval of performance metrics, management of analysis metadata, access to raw query text and command-line arguments, and control flow decisions regarding the continuation of the analysis pipeline based on issue severity.

## API

### `GetIssues`
Retrieves a collection of detected performance issues from the application host context.
*   **Parameters**: Takes the host instance as the `this` parameter.
*   **Returns**: `IEnumerable<PerformanceIssue>` containing the identified issues.
*   **Throws**: May throw if the host context is uninitialized or if the underlying analysis pipeline has not executed.

### `HasCriticalIssues`
Determines whether the current analysis context contains any issues classified as critical.
*   **Parameters**: Takes the host instance as the `this` parameter.
*   **Returns**: `bool` indicating `true` if critical issues exist, otherwise `false`.
*   **Throws**: Unlikely to throw unless the internal issue store is corrupted or inaccessible.

### `SetMetadata`
Associates a specific metadata object with the application host context for later retrieval or logging.
*   **Parameters**: Takes the host instance as the `this` parameter and the metadata object to store.
*   **Returns**: `void`.
*   **Throws**: May throw `ArgumentNullException` if the provided metadata is null, or if the host does not support metadata storage.

### `GetMetadata<T>`
Retrieves a previously stored metadata object of the specified type from the application host context.
*   **Parameters**: Takes the host instance as the `this` parameter.
*   **Returns**: `T?` representing the metadata instance, or `null` if no metadata of type `T` is found.
*   **Throws**: May throw if the host context is invalid.

### `GetPerformanceScoreString`
Generates a formatted string representation of the overall performance score calculated during the analysis.
*   **Parameters**: Takes the host instance as the `this` parameter.
*   **Returns**: `string` containing the human-readable score.
*   **Throws**: May throw if the scoring engine has not produced a result yet.

### `GetIssueCountsBySeverity`
Aggregates the detected issues and returns a count grouped by their severity level.
*   **Parameters**: Takes the host instance as the `this` parameter.
*   **Returns**: `IReadOnlyDictionary<IssueSeverity, int>` mapping each severity level to its occurrence count.
*   **Throws**: Unlikely to throw; returns an empty dictionary if no issues are present.

### `ShouldContinueAnalysis`
Evaluates the current state of detected issues to determine if the analysis pipeline should proceed or terminate early.
*   **Parameters**: Takes the host instance as the `this` parameter.
*   **Returns**: `bool` indicating whether execution should continue.
*   **Throws**: May throw if the termination policy configuration is missing or invalid.

### `GetQueryText`
Extracts the raw SQL query text currently being analyzed from the host context.
*   **Parameters**: Takes the host instance as the `this` parameter.
*   **Returns**: `string` containing the SQL query.
*   **Throws**: May throw if the query text has not been loaded into the context.

### `GetCommandLineArguments`
Retrieves the parsed command-line arguments used to initialize the current application session.
*   **Parameters**: Takes the host instance as the `this` parameter.
*   **Returns**: `CommandLineArguments` object containing the parsed flags and options.
*   **Throws**: May throw if the argument parsing phase failed or was skipped.

## Usage

The following example demonstrates how to retrieve performance issues and check for critical failures before deciding whether to proceed with further processing.

```csharp
using SqlQueryAnalyzer.Extensions;

public void ProcessAnalysis(IApplicationHost host)
{
    // Retrieve all detected performance issues
    var issues = host.GetIssues();
    
    // Check for critical blockers
    if (host.HasCriticalIssues())
    {
        Console.WriteLine("Analysis halted due to critical performance issues.");
        return;
    }

    // Output the performance score
    string score = host.GetPerformanceScoreString();
    Console.WriteLine($"Current Performance Score: {score}");
    
    // Continue with non-critical processing
    foreach (var issue in issues)
    {
        Console.WriteLine($"Found issue: {issue.Description}");
    }
}
```

The next example illustrates storing custom metadata and retrieving it later, alongside accessing the original query text and argument configuration.

```csharp
using SqlQueryAnalyzer.Extensions;
using SqlQueryAnalyzer.Models;

public void EnrichContext(IApplicationHost host, string analysisId)
{
    // Store custom metadata for the session
    var contextData = new AnalysisContext { Id = analysisId, Timestamp = DateTime.UtcNow };
    host.SetMetadata(contextData);

    // Retrieve the metadata later in the pipeline
    var retrievedData = host.GetMetadata<AnalysisContext>();
    if (retrievedData != null)
    {
        Console.WriteLine($"Resuming analysis for ID: {retrievedData.Id}");
    }

    // Access raw inputs
    string query = host.GetQueryText();
    var args = host.GetCommandLineArguments();

    if (args.Verbose)
    {
        Console.WriteLine($"Analyzing query: {query}");
    }
}
```

## Notes

*   **Context Dependency**: All methods in this class rely on the internal state of the `IApplicationHost` instance. Invoking these methods before the host has completed its initialization or analysis phases may result in exceptions or null returns.
*   **Thread Safety**: As these methods operate on the mutable state of a shared host instance, they are not inherently thread-safe. Concurrent calls to `SetMetadata` or state-modifying operations from multiple threads without external synchronization may lead to race conditions.
*   **Generic Metadata**: The `GetMetadata<T>` method performs a type cast on the stored object. If an object of a different type was stored under the same key or context, the method will return `null` rather than throwing an invalid cast exception, adhering to the nullable return type pattern.
*   **Empty Collections**: Methods returning collections (`GetIssues`, `GetIssueCountsBySeverity`) will return empty enumerables or dictionaries rather than `null` if no data is available, preventing null-reference errors in iteration logic.
