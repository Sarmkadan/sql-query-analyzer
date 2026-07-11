# IResultFormatter

The `IResultFormatter` interface defines the contract for formatting SQL query execution results within the `sql-query-analyzer` project. It provides standardized methods to convert raw data sets and batches into specific string representations, enabling flexible output generation for various reporting and analysis scenarios. Implementations of this interface are responsible for handling the serialization logic required to present query outcomes in a human-readable or machine-processable format.

## API

### Format
Converts a single query result set into a formatted string representation.
*   **Purpose**: Transforms an individual result object into the specific format defined by the implementation (e.g., JSON, CSV, Table).
*   **Parameters**: Accepts the result object to be formatted (specific parameter type depends on the concrete implementation).
*   **Return Value**: Returns a `string` containing the formatted output.
*   **Exceptions**: May throw an exception if the input result is null, malformed, or incompatible with the expected format structure.

### FormatBatch
Converts a collection of query result sets (a batch) into a single formatted string.
*   **Purpose**: Aggregates and formats multiple result sets executed in a single batch operation, preserving order and separation as defined by the formatter.
*   **Parameters**: Accepts a collection or array of result objects to be processed together.
*   **Return Value**: Returns a `string` containing the concatenated and formatted output for the entire batch.
*   **Exceptions**: May throw an exception if the batch collection is null, empty (depending on implementation rules), or contains invalid result entries.

### GetFormatType
Retrieves the identifier or name of the format produced by this formatter.
*   **Purpose**: Identifies the specific output format (e.g., "JSON", "XML", "Markdown") supported by the current implementation.
*   **Parameters**: None.
*   **Return Value**: Returns a `string` representing the format type name.
*   **Exceptions**: Generally does not throw exceptions unless the internal state of the formatter is corrupted.

## Usage

### Example 1: Formatting a Single Query Result
The following example demonstrates how to use an implementation of `IResultFormatter` to format a single result set returned from a query execution.

```csharp
using SqlQueryAnalyzer.Formatters;
using SqlQueryAnalyzer.Models;

public void ProcessSingleResult(IResultFormatter formatter, QueryResult result)
{
    if (result == null)
    {
        throw new ArgumentNullException(nameof(result));
    }

    // Retrieve the format type for logging or validation
    string type = formatter.GetFormatType();
    Console.WriteLine($"Processing result as {type}");

    // Format the single result
    string output = formatter.Format(result);
    
    // Output to console or file
    Console.WriteLine(output);
}
```

### Example 2: Formatting a Batch of Results
This example illustrates handling multiple results generated from a batched SQL execution, utilizing the `FormatBatch` method to produce a unified output.

```csharp
using SqlQueryAnalyzer.Formatters;
using SqlQueryAnalyzer.Models;
using System.Collections.Generic;

public void ProcessBatchResults(IResultFormatter formatter, IEnumerable<QueryResult> results)
{
    var resultList = new List<QueryResult>(results);
    
    if (resultList.Count == 0)
    {
        Console.WriteLine("No results to format.");
        return;
    }

    // Format the entire batch at once
    string batchOutput = formatter.FormatBatch(resultList);

    // Verify the format type matches expectations
    if (formatter.GetFormatType() == "JSON")
    {
        // Specific logic for JSON output
        System.IO.File.WriteAllText("batch_results.json", batchOutput);
    }
    else
    {
        System.IO.File.WriteAllText("batch_results.txt", batchOutput);
    }
}
```

## Notes

*   **Thread Safety**: The interface definition does not enforce thread safety. Implementations such as `JsonResultFormatter` should be assumed **not** to be thread-safe unless explicitly documented otherwise by the concrete class. If multiple threads access the same formatter instance concurrently, external synchronization is required.
*   **Null Handling**: Callers must ensure that arguments passed to `Format` and `FormatBatch` are not null. While specific implementations may handle nulls gracefully, the standard contract implies that invalid input will result in an exception.
*   **Format Consistency**: The string returned by `GetFormatType` should remain constant for the lifetime of the formatter instance. Consumers relying on this value for routing or file extension logic should cache it if performance is critical, rather than calling the method repeatedly within tight loops.
*   **Batch Integrity**: When using `FormatBatch`, the order of results in the output string corresponds strictly to the order of the input collection. Reordering must be performed on the collection prior to invoking the formatter.
