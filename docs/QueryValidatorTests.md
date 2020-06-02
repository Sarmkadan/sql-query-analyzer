# QueryValidatorTests
The `QueryValidatorTests` class is a test suite designed to validate the functionality of query validation and sanitization in the sql-query-analyzer project. It provides a comprehensive set of tests to ensure that queries are properly validated, sanitized, and processed according to the project's requirements.

## API
The `QueryValidatorTests` class contains the following public members:
* `IsValidQuery_WellFormedSelectStatement_ReturnsTrue`: Verifies that a well-formed SELECT statement is considered a valid query.
* `IsValidQuery_EmptyString_ReturnsFalse`: Tests that an empty string is not considered a valid query.
* `IsValidQuery_TextWithNoRecognisedSqlKeyword_ReturnsFalse`: Checks that a string without any recognized SQL keywords is not considered a valid query.
* `ValidateDatabaseQuery_NullArgument_ThrowsValidationException`: Ensures that a `ValidationException` is thrown when a null argument is passed to the `ValidateDatabaseQuery` method.
* `SanitizeQueryForDisplay_QueryLongerThanMaxLength_TruncatesWithEllipsis`: Verifies that a query longer than the maximum length is truncated with an ellipsis for display purposes.
* `GenerateQueryKey_SameQueryWithVariableWhitespace_ProducesSameKey`: Tests that the same query with variable whitespace produces the same key.
* `GenerateResultKey_ValidQuery_KeyHasResultPrefix`: Checks that a valid query generates a key with the result prefix.
* `ValidateQuery_CustomRuleRegistered_RuleIsInvokedExactlyOnce`: Ensures that a custom rule registered for query validation is invoked exactly once.

## Usage
Here are two examples of using the `QueryValidatorTests` class in C#:
```csharp
// Example 1: Validating a well-formed SELECT statement
QueryValidatorTests validatorTests = new QueryValidatorTests();
validatorTests.IsValidQuery_WellFormedSelectStatement_ReturnsTrue();

// Example 2: Sanitizing a query for display
string longQuery = "SELECT * FROM table1, table2, table3, table4, table5";
QueryValidatorTests sanitizer = new QueryValidatorTests();
sanitizer.SanitizeQueryForDisplay_QueryLongerThanMaxLength_TruncatesWithEllipsis();
```

## Notes
When using the `QueryValidatorTests` class, note that the `ValidateDatabaseQuery_NullArgument_ThrowsValidationException` test will throw a `ValidationException` if a null argument is passed. Additionally, the `SanitizeQueryForDisplay_QueryLongerThanMaxLength_TruncatesWithEllipsis` test truncates queries longer than the maximum length with an ellipsis, which may affect display purposes. The `GenerateQueryKey_SameQueryWithVariableWhitespace_ProducesSameKey` test ensures that the same query with variable whitespace produces the same key, which is useful for caching and comparison purposes. The `QueryValidatorTests` class is designed to be thread-safe, but it is recommended to use a fresh instance for each test to avoid any potential concurrency issues.
