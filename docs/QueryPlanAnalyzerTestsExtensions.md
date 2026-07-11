# QueryPlanAnalyzerTestsExtensions

The `QueryPlanAnalyzerTestsExtensions` class provides a set of static extension methods designed to streamline unit testing and validation logic within the `sql-query-analyzer` project. It encapsulates common assertions for query plan parsing validity, error handling for malformed plans, and asynchronous execution of specific performance analysis scenarios such as table scan detection and missing index identification. By centralizing these verification routines, the class reduces boilerplate code in test suites and ensures consistent error reporting across different test cases.

## API

### `VerifyInvalidQueryPlanThrows`
Validates that a specific input triggers an exception during query plan analysis, ensuring the system correctly rejects malformed or unsupported plan structures.
- **Purpose**: Asserts that parsing or analyzing an invalid query plan results in a thrown exception.
- **Parameters**: Accepts the invalid query plan data or the action delegate intended to fail (specific signature depends on implementation context, typically `string` or `Action`).
- **Return Value**: `void`.
- **Throws**: Fails the test assertion if the provided input does not result in an exception; otherwise, allows the expected exception to propagate or be caught by the test framework.

### `VerifyValidPlanParsing`
Confirms that a given query plan string or object is successfully parsed without errors.
- **Purpose**: Ensures the analyzer can ingest and structure a valid execution plan.
- **Parameters**: Takes the valid query plan representation (typically `string` or `XElement`).
- **Return Value**: `void`.
- **Throws**: Throws an assertion failure if the parsing process encounters any errors or returns a null/invalid result.

### `RunTableScansTestAsync`
Executes an asynchronous test routine specifically targeting the detection of table scan operations within a query plan.
- **Purpose**: Validates the analyzer's ability to identify full table scans, which are often indicative of performance issues.
- **Parameters**: Requires the query plan data and potentially configuration context for the analysis.
- **Return Value**: `Task`. Completes when the verification logic finishes.
- **Throws**: Throws an exception if table scans are not detected where expected, or if the asynchronous operation fails due to invalid input.

### `RunMissingIndexesTestAsync`
Executes an asynchronous test routine to verify the detection of missing index recommendations based on the provided query plan.
- **Purpose**: Confirms that the analyzer correctly identifies opportunities for index creation to optimize query performance.
- **Parameters**: Requires the query plan data and potentially threshold configurations for index suggestions.
- **Return Value**: `Task`. Completes when the analysis and assertion logic conclude.
- **Throws**: Throws an exception if expected missing index warnings are not generated or if the analysis task faults.

## Usage

The following examples demonstrate how to utilize these extensions within a standard xUnit or NUnit test class to validate query plan analysis behaviors.

```csharp
using System.Threading.Tasks;
using Xunit;
using SqlQueryAnalyzer.Tests.Extensions;

public class QueryPlanValidationTests
{
    [Fact]
    public void Parsing_InvalidXmlStructure_ShouldThrow()
    {
        string malformedPlan = "<ShowPlanXML><InvalidNode></ShowPlanXML>";
        
        // Verifies that the analyzer throws an exception for malformed XML
        QueryPlanAnalyzerTestsExtensions.VerifyInvalidQueryPlanThrows(malformedPlan);
    }

    [Fact]
    public void Parsing_ValidPlan_ShouldSucceed()
    {
        string validPlan = File.ReadAllText("TestData/SimpleSeekPlan.xml");
        
        // Verifies that a known good plan parses without error
        QueryPlanAnalyzerTestsExtensions.VerifyValidPlanParsing(validPlan);
    }
}
```

```csharp
using System.Threading.Tasks;
using Xunit;
using SqlQueryAnalyzer.Tests.Extensions;

public class PerformanceAnalysisTests
{
    [Fact]
    public async Task Analysis_DetectsTableScans()
    {
        string planWithScans = File.ReadAllText("TestData/FullTableScanPlan.xml");
        
        // Asynchronously verifies that table scans are correctly identified
        await QueryPlanAnalyzerTestsExtensions.RunTableScansTestAsync(planWithScans);
    }

    [Fact]
    public async Task Analysis_SuggestsMissingIndexes()
    {
        string planNeedingIndexes = File.ReadAllText("TestData/HighCostLookupPlan.xml");
        
        // Asynchronously verifies that missing index recommendations are generated
        await QueryPlanAnalyzerTestsExtensions.RunMissingIndexesTestAsync(planNeedingIndexes);
    }
}
```

## Notes

- **Thread Safety**: As all members are static and the asynchronous methods (`RunTableScansTestAsync`, `RunMissingIndexesTestAsync`) rely on standard `Task` patterns without shared mutable static state, these methods are generally thread-safe for concurrent invocation within a test suite. However, care should be taken if the underlying analyzer instances they utilize maintain internal state that is not thread-local.
- **Exception Handling**: The `VerifyInvalidQueryPlanThrows` method is designed to wrap execution in a try-catch block internally to assert failure. If the passed action does not throw, this method will trigger a test failure immediately rather than returning a boolean result.
- **Asynchronous Execution**: The `Run...Async` methods must be awaited in the test context. Failure to await these tasks will result in the test completing before the verification logic executes, potentially leading to false positives.
- **Input Validation**: These methods assume the input query plan strings are non-null. Passing `null` will likely result in an `ArgumentNullException` before any specific plan analysis logic occurs.
