# QueryPlanAnalyzerTests

The `QueryPlanAnalyzerTests` class serves as the primary test suite for validating the functionality of the SQL query plan analysis components within the `sql-query-analyzer` project. It encapsulates a series of unit tests designed to verify the correct parsing of execution plan XML, the identification of performance bottlenecks such as table scans, and the generation of missing index recommendations, while ensuring that invalid inputs trigger appropriate exception handling mechanisms.

## API

### `AnalyzeQueryPlan_InvalidQueryPlan_ThrowsException`
Validates the robustness of the analyzer when processing malformed or unsupported query plan structures. This test method asserts that invoking the analysis logic with an invalid query plan input results in the immediate throwing of an exception, preventing silent failures or undefined behavior during runtime. It does not accept parameters and returns void, serving strictly as a verification step for error handling pathways.

### `ParseExecutionPlanAsync_ValidXmlPlan_ReturnsQueryPlan`
Verifies the successful deserialization and parsing of a well-formed XML execution plan. When provided with valid XML input representing a query execution plan, this test ensures that the asynchronous parsing operation completes successfully and returns a populated `QueryPlan` object containing the expected structural data. It takes no explicit parameters, relying on internal test fixtures for the XML payload, and returns void upon successful assertion.

### `ParseExecutionPlanAsync_InvalidXml_ThrowsQueryPlanException`
Ensures that the parsing logic correctly identifies and rejects malformed XML inputs. This test confirms that when the input XML does not conform to the expected schema or is syntactically incorrect, the `ParseExecutionPlanAsync` operation throws a specific `QueryPlanException`. This distinguishes parsing errors from general system exceptions, allowing callers to handle plan-specific issues gracefully. The method returns void and requires no external parameters.

### `GetTableScans_WithTableScans_ReturnsTableScans`
Asynchronously validates the detection logic for table scan operations within an execution plan. When executed against a query plan containing explicit table scan operators, this test asserts that the `GetTableScans` method returns a non-empty collection accurately reflecting the identified scans. It returns a `Task` that completes when the verification is done, ensuring that the analysis correctly traverses the plan tree to locate performance-intensive operations.

### `GetMissingIndexes_WithTableScans_ReturnsRecommendations`
Tests the recommendation engine's ability to suggest missing indexes based on detected table scans. This asynchronous test verifies that when a plan contains table scans that could be optimized via indexing, the `GetMissingIndexes` method yields a list of actionable index recommendations. It returns a `Task` and ensures that the correlation between observed scan patterns and generated optimization advice is functionally correct.

## Usage

The following examples demonstrate how the logic verified by this test class might be consumed in a production context or integrated into a larger testing strategy.

**Example 1: Validating XML Parsing and Error Handling**
This example illustrates the try-catch pattern required to handle both valid plans and the `QueryPlanException` verified by the test suite.

```csharp
using System;
using System.Threading.Tasks;
using SqlQueryAnalyzer;
using SqlQueryAnalyzer.Exceptions;

public class PlanProcessor
{
    private readonly IQueryPlanParser _parser;

    public PlanProcessor(IQueryPlanParser parser)
    {
        _parser = parser;
    }

    public async Task ProcessPlanAsync(string xmlContent)
    {
        try
        {
            var plan = await _parser.ParseExecutionPlanAsync(xmlContent);
            Console.WriteLine($"Plan parsed successfully. Root Operator: {plan.RootOperator.Name}");
        }
        catch (QueryPlanException ex)
        {
            // Handles the specific exception case validated by ParseExecutionPlanAsync_InvalidXml_ThrowsQueryPlanException
            Console.Error.WriteLine($"Failed to parse execution plan: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
        }
    }
}
```

**Example 2: Retrieving Optimization Recommendations**
This example demonstrates the asynchronous retrieval of table scans and missing index recommendations, corresponding to the behaviors verified in `GetTableScans_WithTableScans_ReturnsTableScans` and `GetMissingIndexes_WithTableScans_ReturnsRecommendations`.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SqlQueryAnalyzer;

public class OptimizationService
{
    private readonly IQueryPlanAnalyzer _analyzer;

    public OptimizationService(IQueryPlanAnalyzer analyzer)
    {
        _analyzer = analyzer;
    }

    public async Task<List<string>> GetOptimizationSuggestionsAsync(QueryPlan plan)
    {
        var suggestions = new List<string>();

        // Retrieve table scans identified in the plan
        var tableScans = await _analyzer.GetTableScans(plan);
        foreach (var scan in tableScans)
        {
            suggestions.Add($"Detected table scan on table: {scan.TableName}");
        }

        // Retrieve missing index recommendations based on the scans
        var missingIndexes = await _analyzer.GetMissingIndexes(plan);
        foreach (var recommendation in missingIndexes)
        {
            suggestions.Add($"Recommendation: {recommendation.CreateIndexStatement}");
        }

        return suggestions;
    }
}
```

## Notes

*   **Asynchronous Execution**: Members `GetTableScans_WithTableScans_ReturnsTableScans` and `GetMissingIndexes_WithTableScans_ReturnsRecommendations` imply that the underlying analysis operations are asynchronous (`Task`-returning). Callers must await these operations to avoid blocking threads, particularly when processing large or complex execution plans.
*   **Exception Specificity**: The distinction between `ParseExecutionPlanAsync_InvalidXml_ThrowsQueryPlanException` and `AnalyzeQueryPlan_InvalidQueryPlan_ThrowsException` indicates that the system differentiates between XML parsing failures (throwing `QueryPlanException`) and logical analysis failures on invalid plan structures (throwing a generic or different specific exception). Implementations should catch specific exception types rather than relying solely on base `Exception` handling where possible.
*   **Thread Safety**: As this class represents a test suite, the methods themselves are stateless regarding shared mutable data between test runs. However, the underlying components being tested (`IQueryPlanParser`, `IQueryPlanAnalyzer`) should be assumed to be stateless or thread-safe if instantiated as singletons, given the asynchronous nature of the verified methods.
*   **Input Validation**: The tests explicitly cover edge cases involving invalid XML and invalid query plan structures. Production code consuming these utilities should ensure that input strings are not null or empty before invocation to prevent argument exceptions prior to the specific validation logic tested here.
