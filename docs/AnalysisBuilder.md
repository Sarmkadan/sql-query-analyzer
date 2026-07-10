# AnalysisBuilder

A builder class used to construct and configure SQL query analysis requests for the `sql-query-analyzer` project. It provides a fluent interface to specify analysis parameters such as queries, applications, procedures, and analysis modes before building the final `AnalysisRequestDto` payload.

## API

### `AnalysisBuilder WithQuery(string query)`
Configures the SQL query text to be analyzed. The query is stored for subsequent analysis steps.

- **Parameters**
  - `query` – The SQL query string to analyze.
- **Return value**
  - Returns the current `AnalysisBuilder` instance for method chaining.
- **Exceptions**
  - Throws `ArgumentNullException` if `query` is `null`.

---

### `AnalysisBuilder WithApplication(string applicationName)`
Specifies the application context under which the query executes. Used to contextualize analysis results.

- **Parameters**
  - `applicationName` – The name of the application.
- **Return value**
  - Returns the current `AnalysisBuilder` instance for method chaining.
- **Exceptions**
  - Throws `ArgumentNullException` if `applicationName` is `null`.

---
### `AnalysisBuilder WithProcedure(string procedureName)`
Sets the stored procedure name associated with the query. Aids in procedure-specific analysis.

- **Parameters**
  - `procedureName` – The name of the stored procedure.
- **Return value**
  - Returns the current `AnalysisBuilder` instance for method chaining.
- **Exceptions**
  - Throws `ArgumentNullException` if `procedureName` is `null`.

---
### `AnalysisBuilder WithModule(string moduleName)`
Defines the module or schema context for the query. Useful for module-level analysis.

- **Parameters**
  - `moduleName` – The module or schema name.
- **Return value**
  - Returns the current `AnalysisBuilder` instance for method chaining.
- **Exceptions**
  - Throws `ArgumentNullException` if `moduleName` is `null`.

---
### `AnalysisBuilder IncludeIndexSuggestions()`
Enables inclusion of index suggestion diagnostics in the final analysis report.

- **Return value**
  - Returns the current `AnalysisBuilder` instance for method chaining.
- **Note**
  - This is a toggle-style method; calling it multiple times has no additional effect.

---
### `AnalysisBuilder AnalyzeFragmentation()`
Requests analysis of index fragmentation as part of the query evaluation.

- **Return value**
  - Returns the current `AnalysisBuilder` instance for method chaining.
- **Note**
  - This is a toggle-style method; calling it multiple times has no additional effect.

---
### `AnalysisBuilder AnalyzePlan()`
Instructs the analyzer to generate and evaluate the query execution plan.

- **Return value**
  - Returns the current `AnalysisBuilder` instance for method chaining.
- **Note**
  - This is a toggle-style method; calling it multiple times has no additional effect.

---
### `AnalysisBuilder WithExecutionPlan(string executionPlanXml)`
Provides an externally generated execution plan in XML format for analysis.

- **Parameters**
  - `executionPlanXml` – The execution plan XML string.
- **Return value**
  - Returns the current `AnalysisBuilder` instance for method chaining.
- **Exceptions**
  - Throws `ArgumentNullException` if `executionPlanXml` is `null`.

---
### `AnalysisRequestDto Build()`
Finalizes the configuration and constructs the immutable `AnalysisRequestDto` payload.

- **Return value**
  - Returns a new `AnalysisRequestDto` instance populated with the current builder state.
- **Exceptions**
  - Throws `InvalidOperationException` if no query has been set via `WithQuery`.

---
### `AnalysisBuilder Reset()`
Clears all previously configured settings, returning the builder to its initial state.

- **Return value**
  - Returns the current `AnalysisBuilder` instance for method chaining.

---
### `AnalysisBuilder Full()`
Configures the analysis to run in full diagnostic mode, enabling all available checks.

- **Return value**
  - Returns the current `AnalysisBuilder` instance for method chaining.
- **Note**
  - This overrides any previous mode-specific calls (e.g., `Quick`).

---
### `AnalysisBuilder Quick()`
Sets the analysis to run in a lightweight, fast mode with reduced diagnostics.

- **Return value**
  - Returns the current `AnalysisBuilder` instance for method chaining.
- **Note**
  - This overrides any previous mode-specific calls (e.g., `Full`).

---
### `List<string> GetErrors()`
Retrieves the list of validation or configuration errors accumulated during builder usage.

- **Return value**
  - Returns a `List<string>` of error messages. Empty if no errors are present.

---
### `bool IsValid`
Indicates whether the current builder configuration is valid and ready for building.

- **Return value**
  - Returns `true` if the configuration is valid; otherwise, `false`.

---
### `BatchAnalysisBuilder AddQuery(string query)`
Adds a single SQL query to a batch analysis configuration.

- **Parameters**
  - `query` – The SQL query string to add.
- **Return value**
  - Returns the associated `BatchAnalysisBuilder` instance for method chaining.
- **Exceptions**
  - Throws `ArgumentNullException` if `query` is `null`.

---
### `BatchAnalysisBuilder AddQueries(IEnumerable<string> queries)`
Adds multiple SQL queries to a batch analysis configuration.

- **Parameters**
  - `queries` – An enumerable of SQL query strings.
- **Return value**
  - Returns the associated `BatchAnalysisBuilder` instance for method chaining.
- **Exceptions**
  - Throws `ArgumentNullException` if `queries` is `null`.

---
### `BatchAnalysisBuilder WithApplication(string applicationName)`
Specifies the application context for the entire batch of queries.

- **Parameters**
  - `applicationName` – The name of the application.
- **Return value**
  - Returns the associated `BatchAnalysisBuilder` instance for method chaining.
- **Exceptions**
  - Throws `ArgumentNullException` if `applicationName` is `null`.

---
### `BatchAnalysisBuilder AnalyzePatterns()`
Enables pattern-based analysis across the batch of queries.

- **Return value**
  - Returns the associated `BatchAnalysisBuilder` instance for method chaining.
- **Note**
  - This is a toggle-style method; calling it multiple times has no additional effect.

---
### `BatchAnalysisBuilder WithTimeout(int timeoutSeconds)`
Sets the maximum allowed execution time for the analysis operations.

- **Parameters**
  - `timeoutSeconds` – Timeout duration in seconds.
- **Return value**
  - Returns the associated `BatchAnalysisBuilder` instance for method chaining.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `timeoutSeconds` is less than zero.

## Usage
