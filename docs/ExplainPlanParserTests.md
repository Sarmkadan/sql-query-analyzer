# ExplainPlanParserTests

The `ExplainPlanParserTests` class provides a comprehensive suite of unit tests designed to validate the reliability and robustness of the `ExplainPlanParser` component within the `sql-query-analyzer` framework. Its primary purpose is to ensure that PostgreSQL query execution plans, delivered in various JSON formats, are accurately parsed and mapped to the application's internal data structures, while also verifying the parser's error-handling behavior when encountering malformed or unexpected input.

## API

### ParsePostgreSqlPlan_Postgres17Format_ReturnsCorrectPlan
Asynchronously verifies that the parser correctly deserializes a valid JSON query execution plan compliant with the PostgreSQL 17 format. It asserts that the resulting internal representation accurately reflects the hierarchy and attributes of the original plan.

### ParsePostgreSqlPlan_EmptyJson_ReturnsDefaultPlan
Asynchronously validates that the parser handles an empty JSON input string gracefully by returning a pre-defined default plan object, ensuring that the application does not throw exceptions when presented with empty data payloads.

### ParsePostgreSqlPlan_InvalidJson_LogsErrorAndReturnsDefaultPlan
Asynchronously tests the parser's resilience when provided with malformed or invalid JSON input. It verifies that the parser logs the corresponding error through the configured logging mechanism and safely returns a default plan object instead of propagating a parsing exception.

## Usage

```csharp
// Example 1: Executing tests via the dotnet test command
// Navigate to the project root and run:
dotnet test --filter "FullyQualifiedName~ExplainPlanParserTests"

// Example 2: Invoking a test case programmatically within a test class
public class ExplainPlanIntegrationTests
{
    private readonly ExplainPlanParserTests _parserTests = new();

    [Fact]
    public async Task ValidatePostgres17Parsing()
    {
        // Executes the specific test case for PostgreSQL 17 format
        await _parserTests.ParsePostgreSqlPlan_Postgres17Format_ReturnsCorrectPlan();
    }
}
```

## Notes

*   **Execution Isolation:** These tests are built for the xUnit framework and are designed to run in isolation. Each test method manages its own parser instance to prevent state leakage between test executions.
*   **Thread Safety:** While the `ExplainPlanParser` itself should be stateless and thread-safe, these test methods are inherently asynchronous and execute on the thread pool. They do not share resources that require locking mechanisms, assuming standard test runner configurations.
*   **Logging Dependencies:** The `ParsePostgreSqlPlan_InvalidJson_LogsErrorAndReturnsDefaultPlan` test relies on the configured logging infrastructure of the host project. If the logging output is redirected or captured, ensure the test environment is correctly configured to monitor log events.
