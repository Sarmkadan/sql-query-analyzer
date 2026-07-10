# ExplainPlanParserService

A service that parses execution plans from SQL Server, PostgreSQL, and MySQL into a unified `QueryPlan` model and extracts key performance metrics from those plans.

## API

### `ExplainPlanParserService`

Initializes a new instance of the parser service. No external dependencies are required; the service manages its own parsing logic and state internally.

### `ParseSqlServerPlanAsync`

Parses a SQL Server execution plan XML into a structured `QueryPlan` object.

- **Parameters**
  - `planXml` (`string`): The raw SQL Server execution plan XML content.
- **Return Value**
  - `Task<QueryPlan>`: A task that resolves to a populated `QueryPlan` instance representing the parsed plan.
- **Exceptions**
  - Throws `ArgumentNullException` if `planXml` is `null`.
  - Throws `FormatException` if the XML is malformed or does not conform to SQL Server execution plan schema.
  - Throws `InvalidOperationException` if parsing fails due to unsupported plan features or schema changes.

### `ParsePostgreSqlPlanAsync`

Parses a PostgreSQL execution plan JSON into a structured `QueryPlan` object.

- **Parameters**
  - `planJson` (`string`): The raw PostgreSQL execution plan JSON content.
- **Return Value**
  - `Task<QueryPlan>`: A task that resolves to a populated `QueryPlan` instance representing the parsed plan.
- **Exceptions**
  - Throws `ArgumentNullException` if `planJson` is `null`.
  - Throws `FormatException` if the JSON is malformed or does not conform to PostgreSQL execution plan schema.
  - Throws `InvalidOperationException` if parsing fails due to unsupported plan features or schema changes.

### `ParseMySqlPlanAsync`

Parses a MySQL execution plan JSON into a structured `QueryPlan` object.

- **Parameters**
  - `planJson` (`string`): The raw MySQL execution plan JSON content.
  - `explainFormat` (`MySqlExplainFormat`, optional): The format of the plan (e.g., `Traditional`, `JSON`). Defaults to `Traditional`.
- **Return Value**
  - `Task<QueryPlan>`: A task that resolves to a populated `QueryPlan` instance representing the parsed plan.
- **Exceptions**
  - Throws `ArgumentNullException` if `planJson` is `null`.
  - Throws `FormatException` if the JSON is malformed or does not conform to MySQL execution plan schema.
  - Throws `InvalidOperationException` if parsing fails due to unsupported plan features or schema changes.
  - Throws `ArgumentOutOfRangeException` if `explainFormat` is not a valid enum value.

### `ExtractPlanMetricsAsync`

Extracts a dictionary of key performance metrics from a previously parsed `QueryPlan`.

- **Parameters**
  - `plan` (`QueryPlan`): The parsed query plan from which to extract metrics.
- **Return Value**
  - `Task<Dictionary<string, object>>`: A task that resolves to a dictionary of metric names and their values (e.g., `"cpuTimeMs"`, `"reads"`).
- **Exceptions**
  - Throws `ArgumentNullException` if `plan` is `null`.
  - Throws `InvalidOperationException` if the plan contains unsupported or unparseable metric nodes.

## Usage
