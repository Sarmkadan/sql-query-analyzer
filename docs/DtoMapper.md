# DtoMapper

A utility class for mapping SQL query analysis results into various data transfer objects (DTOs) used to communicate findings to clients. It provides static methods to convert analysis results into structured response DTOs, facilitating consistent serialization and client consumption.

## API

### `AnalysisResponseDto ToResponseDto(AnalysisResult result)`

Maps an `AnalysisResult` object into an `AnalysisResponseDto` containing high-level analysis findings. The returned DTO includes aggregated metrics such as table count, join count, and query complexity indicators.

- **Parameters**: `result` – The analysis result containing raw query and structural data.
- **Returns**: An `AnalysisResponseDto` populated with derived metrics and findings.
- **Throws**: `ArgumentNullException` if `result` is `null`.

---

### `PerformanceIssueDto ToIssueDto(PerformanceIssue issue)`

Converts a `PerformanceIssue` instance into a `PerformanceIssueDto` for client reporting. Captures issue type, severity, and contextual details such as affected tables or operations.

- **Parameters**: `issue` – The performance issue to map.
- **Returns**: A `PerformanceIssueDto` with issue metadata.
- **Throws**: `ArgumentNullException` if `issue` is `null`.

---
### `IndexSuggestionDto ToSuggestionDto(IndexSuggestion suggestion)`

Transforms an `IndexSuggestion` into a client-friendly `IndexSuggestionDto`, including suggested index definition, benefit estimation, and affected query fragments.

- **Parameters**: `suggestion` – The index suggestion to serialize.
- **Returns**: An `IndexSuggestionDto` with index design and rationale.
- **Throws**: `ArgumentNullException` if `suggestion` is `null`.

---
### `IndexDetailDto ToIndexDetailDto(IndexDetail detail)`

Maps an `IndexDetail` object into a detailed `IndexDetailDto`, exposing schema, column list, uniqueness, and usage statistics.

- **Parameters**: `detail` – The index detail to convert.
- **Returns**: An `IndexDetailDto` with structural and statistical properties.
- **Throws**: `ArgumentNullException` if `detail` is `null`.

---
### `IndexAnalysisResponseDto ToIndexAnalysisResponseDto(IndexAnalysisResult result)`

Converts an `IndexAnalysisResult` into a response DTO containing index usage analysis and recommendations.

- **Parameters**: `result` – The index analysis result to map.
- **Returns**: An `IndexAnalysisResponseDto` with analysis findings.
- **Throws**: `ArgumentNullException` if `result` is `null`.

---
### `QueryDetailDto ToQueryDetailDto(QueryDetail detail)`

Serializes a `QueryDetail` into a `QueryDetailDto`, including query text, type classification, and structural metrics such as line count and parameter usage.

- **Parameters**: `detail` – The query detail to convert.
- **Returns**: A `QueryDetailDto` with query metadata and metrics.
- **Throws**: `ArgumentNullException` if `detail` is `null`.

---
### `BatchAnalysisResponseDto ToBatchResponseDto(BatchAnalysisResult result)`

Aggregates a `BatchAnalysisResult` into a `BatchAnalysisResponseDto`, combining multiple query analyses into a single response payload.

- **Parameters**: `result` – The batch analysis result to map.
- **Returns**: A `BatchAnalysisResponseDto` containing all individual query analyses.
- **Throws**: `ArgumentNullException` if `result` is `null`.

---
### `string QueryId`

Gets the unique identifier for the query being analyzed. Used to correlate analysis results across requests and logging.

- **Type**: `string`
- **Access**: Read-only

---
### `string QueryText`

Gets the raw SQL query text under analysis. May be truncated or sanitized depending on configuration.

- **Type**: `string`
- **Access**: Read-only

---
### `string QueryType`

Gets the inferred type of the query (e.g., SELECT, INSERT, UPDATE, DELETE, MERGE).

- **Type**: `string`
- **Access**: Read-only

---
### `int TableCount`

Gets the number of distinct tables referenced in the query.

- **Type**: `int`
- **Access**: Read-only

---
### `List<string> Tables`

Gets the list of table names involved in the query.

- **Type**: `List<string>`
- **Access**: Read-only

---
### `int JoinCount`

Gets the number of join operations detected in the query.

- **Type**: `int`
- **Access**: Read-only

---
### `int ParameterCount`

Gets the number of parameters or variables used in the query.

- **Type**: `int`
- **Access**: Read-only

---
### `int LineCount`

Gets the estimated number of logical lines in the query (excluding comments and whitespace).

- **Type**: `int`
- **Access**: Read-only

## Usage
