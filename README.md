// ## QueryValidatorJsonExtensions
// The `QueryValidatorJsonExtensions` class provides methods for converting between JSON and .NET objects related to query validation, such as `QueryAnalysisResult`, `PerformanceIssue`, and `IndexSuggestion`. 
// Here's an example of how to use it:
// ```csharp
// var queryAnalysisResult = new QueryAnalysisResult();
// var json = queryAnalysisResult.ToJson();
// var deserializedResult = QueryValidatorJsonExtensions.FromJsonToAnalysisResult(json);
// var performanceIssue = new PerformanceIssue();
// var performanceIssueJson = performanceIssue.ToJson();
// var deserializedPerformanceIssue = QueryValidatorJsonExtensions.FromJsonToPerformanceIssue(performanceIssueJson);
// var indexSuggestion = new IndexSuggestion();
// var indexSuggestionJson = indexSuggestion.ToJson();
// var deserializedIndexSuggestion = QueryValidatorJsonExtensions.FromJsonToIndexSuggestion(indexSuggestionJson);
// ```
// SqlQueryAnalyzerJsonExtensions
//
// ## SelectStarPluginTests
// The `SelectStarPluginTests` class contains unit tests to verify the functionality of the `SelectStarPlugin`. It ensures that the plugin correctly detects `SELECT *` patterns in queries while ignoring exceptions like `COUNT(*)` or stars within comments.
// Here is an example of how to invoke one of the test methods:
// ```csharp
// var testInstance = new SelectStarPluginTests();
// await testInstance.ProcessAsync_QueryWithSelectStar_AddsIssue();
// ```
//
// ## QueryNormalizerValidationTests
// The `QueryNormalizerValidationTests` class provides a suite of validation helpers that verify the behavior of the query normalizer under various input conditions. It checks how the normalizer handles null, empty, whitespace, and valid queries, as well as its ability to produce parameterized queries and extract table or column names.
// A typical usage pattern creates an instance of the test class and invokes the public validation methods directly:
//
// ```csharp
// var validator = new QueryNormalizerValidationTests();
//
// // Null, empty, and whitespace inputs are rejected.
// validator.TryNormalize_WithNullInput_ReturnsFalseAndNullOutput();
// validator.TryNormalize_WithEmptyInput_ReturnsFalseAndNullOutput();
// validator.TryNormalize_WithWhitespaceInput_ReturnsFalseAndNullOutput();
//
// // A valid query is normalized successfully.
// validator.TryNormalize_WithValidInput_ReturnsTrueAndNormalizedQuery();
//
// // Parameterized query generation.
// validator.TryToParameterizedQuery_WithNullInput_ReturnsFalseAndNullOutput();
// validator.TryToParameterizedQuery_WithEmptyInput_ReturnsFalseAndNullOutput();
// validator.TryToParameterizedQuery_WithValidInput_ReturnsTrueAndParameterizedQuery();
//
// // Table and column name extraction.
// validator.TryExtractTableNames_WithNullInput_ReturnsFalseAndNullOutput();
// validator.TryExtractTableNames_WithEmptyInput_ReturnsFalseAndNullOutput();
// validator.TryExtractTableNames_WithValidInput_ReturnsTrueAndTableNames();
//
// validator.TryExtractColumnNames_WithNullInput_ReturnsFalseAndNullOutput();
// validator.TryExtractColumnNames_WithEmptyInput_ReturnsFalseAndNullOutput();
// validator.TryExtractColumnNames_WithValidInput_ReturnsTrueAndColumnNames();
// ```
// ## UnboundedOrderByPluginTests
// The `UnboundedOrderByPluginTests` class contains unit tests to verify the functionality of the `UnboundedOrderByPlugin`. It checks how the plugin handles queries with `ORDER BY` clauses, ensuring that unbounded queries are correctly identified and issues are added accordingly.
// Here is an example of how to invoke one of the test methods:
// ```csharp
// var testInstance = new UnboundedOrderByPluginTests();
// await testInstance.ProcessAsync_QueryWithOrderByWithoutPagination_AddsIssue();
// ```
