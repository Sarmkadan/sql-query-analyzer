# SqlPatternAnalyzerTestsExtensions

Extension class that provides convenience methods to execute groups of tests or individual test cases in the `SqlPatternAnalyzerTests` class. Designed to simplify test execution workflows and reduce boilerplate when running specific test categories related to SQL pattern analysis.


## API

### ExecuteAllSelectStarTests

Executes all tests related to SELECT * pattern detection and optimization recommendations.

- **Parameters**: `tests` – The `SqlPatternAnalyzerTests` instance on which to execute the tests
- **Return value**: `void`
- **Exceptions**: Throws `ArgumentNullException` if `tests` is null
- **Test cases executed**:
  - `HasSelectStar_QueryContainsStar_ReturnsTrue`
  - `HasSelectStar_QueryWithNamedColumns_ReturnsFalse`
  - `GenerateOptimizationRecommendations_SelectStarQuery_IncludesColumnReplacementAdvice`


### ExecuteAllNPlusOneTests

Executes all tests related to N+1 pattern detection.

- **Parameters**: `tests` – The `SqlPatternAnalyzerTests` instance on which to execute the tests
- **Return value**: `void`
- **Exceptions**: Throws `ArgumentNullException` if `tests` is null
- **Test cases executed**:
  - `DetectNPlusOnePattern_SingleQueryInList_ReturnsFalse`
  - `DetectNPlusOnePattern_SameTableAccessedMoreThanFiveTimes_ReturnsTrue`


### ExecuteAllReadabilityTests

Executes all tests related to readability score calculation.

- **Parameters**: `tests` – The `SqlPatternAnalyzerTests` instance on which to execute the tests
- **Return value**: `void`
- **Exceptions**: Throws `ArgumentNullException` if `tests` is null
- **Test cases executed**:
  - `CalculateReadabilityScore_WellWrittenQuery_ReturnsFullScore`
  - `CalculateReadabilityScore_SelectStarWithImplicitJoin_DeductsThirtyPoints`


### ExecuteAllPatternDetectionTests

Executes all tests related to pattern detection for leading wildcard LIKE clauses.

- **Parameters**: `tests` – The `SqlPatternAnalyzerTests` instance on which to execute the tests
- **Return value**: `void`
- **Exceptions**: Throws `ArgumentNullException` if `tests` is null
- **Test cases executed**:
  - `HasLeadingWildcardLike_PatternStartsWithPercent_ReturnsTrue`


### ExecuteSelectStarDetectionTest

Executes the test that verifies SELECT * detection works correctly.

- **Parameters**: `tests` – The `SqlPatternAnalyzerTests` instance on which to execute the test
- **Return value**: `void`
- **Exceptions**: Throws `ArgumentNullException` if `tests` is null
- **Test case executed**:
  - `HasSelectStar_QueryContainsStar_ReturnsTrue`

### ExecuteSelectStarWithColumnsTest

Executes the test that verifies explicit column selection is detected correctly.

- **Parameters**: `tests` – The `SqlPatternAnalyzerTests` instance on which to execute the test
- **Return value**: `void`
- **Exceptions**: Throws `ArgumentNullException` if `tests` is null
- **Test case executed**:
  - `HasSelectStar_QueryWithNamedColumns_ReturnsFalse`


### ExecuteOptimizationRecommendationsTest

Executes the test that verifies optimization recommendations include column replacement advice.

- **Parameters**: `tests` – The `SqlPatternAnalyzerTests` instance on which to execute the test
- **Return value**: `void`
- **Exceptions**: Throws `ArgumentNullException` if `tests` is null
- **Test case executed**:
  - `GenerateOptimizationRecommendations_SelectStarQuery_IncludesColumnReplacementAdvice`


## Usage

### Example 1: Executing all SELECT * related tests

```csharp
using SqlQueryAnalyzer.Tests;

var analyzerTests = new SqlPatternAnalyzerTests();

// Execute all SELECT * pattern detection and optimization tests
analyzerTests.ExecuteAllSelectStarTests();
```

### Example 2: Running individual test cases in a test suite

```csharp
using SqlQueryAnalyzer.Tests;
using Xunit;

public class PatternAnalyzerTestSuite
{
    [Fact]
    public void RunSelectStarDetectionSuite()
    {
        var tests = new SqlPatternAnalyzerTests();
        
        // Run individual test cases
        tests.ExecuteSelectStarDetectionTest();
        tests.ExecuteSelectStarWithColumnsTest();
        tests.ExecuteOptimizationRecommendationsTest();
    }
    
    [Fact]
    public void RunNPlusOneDetectionSuite()
    {
        var tests = new SqlPatternAnalyzerTests();
        
        // Run all N+1 pattern detection tests
        tests.ExecuteAllNPlusOneTests();
    }
}
```

## Notes

- All extension methods validate their input parameter using `ArgumentNullException.ThrowIfNull` and will throw if the `tests` parameter is null
- Methods are thread-safe as they only perform parameter validation and method invocation on the provided instance
- Extension methods follow the same execution pattern as direct test method calls and will propagate any exceptions thrown by the underlying test methods
- No state is maintained by the extension methods themselves; they are purely functional wrappers around existing test methods
- The methods are designed to work with the `SqlPatternAnalyzerTests` class and will fail at compile time if the target class structure changes
- When using these methods in a test suite, ensure the `SqlPatternAnalyzerTests` instance is properly initialized before calling any extension methods