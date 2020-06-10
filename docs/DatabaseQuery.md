# DatabaseQuery

`DatabaseQuery` represents a fully parsed and analyzed SQL query extracted from source code, database logs, or query repositories. It encapsulates the query text, metadata about its origin and type, and the structural elements discovered during analysis—such as referenced tables and columns, join conditions, filtering clauses, parameters, and variable declarations. This type serves as the primary data transfer object throughout the `sql-query-analyzer` pipeline.

## API

### `QueryId`
`public string QueryId`

A unique identifier assigned to the query instance. This value is generated during ingestion and remains stable for the lifetime of the object. It is used for correlation across analysis steps and for deduplication in storage.

### `QueryText`
`public string QueryText`

The raw SQL text of the query as originally encountered. This string preserves the original formatting, casing, and comments (if capture was configured to retain them). It is the authoritative source text from which all structural members are derived.

### `ProcedureName`
`public string? ProcedureName`

The name of the stored procedure or function containing this query, if the query was extracted from a procedural database object. Null when the query originates from ad-hoc SQL, application code, or other non-procedural sources.

### `ModuleName`
`public string? ModuleName`

The name of the source module or file from which the query was extracted. In application code analysis, this corresponds to the class or file name. In database extraction, this may represent the schema module. Null when the source module cannot be determined.

### `ApplicationName`
`public string? ApplicationName`

The name of the application or service that issued or contains the query. Set when queries are collected from application source code or traced from specific application connections. Null for queries extracted directly from database logs without application context.

### `DatabaseName`
`public string? DatabaseName`

The name of the target database against which the query executes. Null when the database context is ambiguous or when the query was captured without explicit database association.

### `QueryType`
`public QueryType QueryType`

An enumeration value classifying the query by its primary operation. Values include `Select`, `Insert`, `Update`, `Delete`, `Merge`, and `Unknown`. Determined during parsing by inspecting the leading DML statement.

### `DatabaseType`
`public DatabaseType DatabaseType`

An enumeration value indicating the dialect or platform of the SQL text. Values include `SqlServer`, `PostgreSql`, `MySql`, `Oracle`, and `Unknown`. This drives parser selection and affects how structural elements are interpreted.

### `SchemaName`
`public string SchemaName`

The default schema name used for unqualified object references within the query. Extracted from the query context (e.g., `USE` statements, connection defaults) or inferred from the surrounding module.

### `CreatedBy`
`public string CreatedBy`

The identity of the user, service account, or process that originally authored or first submitted the query. Populated from source control attribution, database audit logs, or application tracing headers.

### `CreatedDate`
`public DateTime CreatedDate`

The UTC timestamp when the query was first recorded or ingested into the analysis system. For queries extracted from source code, this may reflect the commit date rather than the system ingestion time.

### `ModifiedBy`
`public string? ModifiedBy`

The identity of the user or process that last modified the query text or metadata. Null when the query has never been modified after initial creation or when modification tracking is unavailable.

### `ModifiedDate`
`public DateTime? ModifiedDate`

The UTC timestamp of the most recent modification to the query text or metadata. Null when no modification has occurred or when modification history is not captured.

### `ReferencedTables`
`public List<string> ReferencedTables`

A list of fully qualified or partially qualified table names referenced in the query. Includes tables appearing in `FROM`, `JOIN`, subqueries, CTEs, and DML target clauses. Each entry preserves the qualification level present in the source text (e.g., `dbo.Orders`, `Orders`). The list is never null; it is empty when no tables are referenced.

### `ReferencedColumns`
`public List<string> ReferencedColumns`

A list of column references found in the query, including columns in `SELECT` lists, `WHERE` predicates, `JOIN` conditions, `GROUP BY` and `ORDER BY` clauses, and set operations. Entries may be qualified with table aliases or names as they appear in the source text. The list is never null.

### `JoinConditions`
`public List<string> JoinConditions`

A list of join predicate expressions extracted from `JOIN ... ON` clauses. Each entry is the raw text of the condition as written. The list is never null; it is empty for queries without joins.

### `WhereConditions`
`public List<string> WhereConditions`

A list of top-level predicate expressions extracted from the `WHERE` clause. Compound conditions joined by `AND` or `OR` at the root level are separated into individual entries. Sub-conditions within parentheses are preserved as single entries. The list is never null.

### `Parameters`
`public Dictionary<string, ParameterInfo> Parameters`

A dictionary mapping parameter names to their metadata. Keys are the parameter names as they appear in the query (including leading `@` for SQL Server, `:` for Oracle, etc.). Values are `ParameterInfo` objects describing the data type, direction, and default value if known. The dictionary is never null; it is empty for queries without parameters.

### `VariableDeclarations`
`public Dictionary<string, string> VariableDeclarations`

A dictionary mapping declared variable names to their declared data types. Captures `DECLARE` statements and local variable definitions within procedural SQL blocks. The type string preserves the source dialect's type syntax. The dictionary is never null.

### `LineCount`
`public int LineCount`

The number of lines in `QueryText`, calculated using the environment's newline conventions. Includes blank lines and comment-only lines. Used for rough complexity estimation and formatting analysis.

## Usage

### Example 1: Inspecting a parsed query from application code

```csharp
DatabaseQuery query = analyzer.ParseQuery(
    sourceCode: "SELECT u.Id, u.Email FROM Users u WHERE u.IsActive = @active",
    applicationName: "AuthService",
    moduleName: "UserRepository.cs",
    databaseType: DatabaseType.SqlServer
);

Console.WriteLine($"Query {query.QueryId}: {query.QueryType}");
Console.WriteLine($"  Tables: {string.Join(", ", query.ReferencedTables)}");
Console.WriteLine($"  Columns: {string.Join(", ", query.ReferencedColumns)}");
Console.WriteLine($"  Conditions: {string.Join("; ", query.WhereConditions)}");

foreach (var param in query.Parameters)
{
    Console.WriteLine($"  Parameter {param.Key}: {param.Value.DataType}");
}
```

### Example 2: Comparing two queries for structural overlap

```csharp
DatabaseQuery queryA = repository.GetById("q-001");
DatabaseQuery queryB = repository.GetById("q-002");

var commonTables = queryA.ReferencedTables
    .Intersect(queryB.ReferencedTables, StringComparer.OrdinalIgnoreCase)
    .ToList();

var commonColumns = queryA.ReferencedColumns
    .Intersect(queryB.ReferencedColumns, StringComparer.OrdinalIgnoreCase)
    .ToList();

if (commonTables.Any() && commonColumns.Any())
{
    Console.WriteLine($"Queries overlap on tables: {string.Join(", ", commonTables)}");
    Console.WriteLine($"Shared columns: {string.Join(", ", commonColumns)}");
}
```

## Notes

All collection-typed members (`ReferencedTables`, `ReferencedColumns`, `JoinConditions`, `WhereConditions`, `Parameters`, `VariableDeclarations`) are guaranteed to be non-null. Consumers can safely iterate over them without null checks. Empty collections indicate absence of the corresponding element in the query text.

`QueryType` may be `Unknown` when the parser encounters malformed SQL, non-DML statements (e.g., `SET`, `USE`), or dialect-specific constructs that do not map cleanly to the standard DML categories. Callers should handle `Unknown` gracefully in switch expressions.

`Parameters` and `VariableDeclarations` use dialect-specific naming conventions for keys. SQL Server parameters include the `@` prefix; Oracle parameters use `:`. Code that correlates parameters across dialects must normalize these prefixes.

`ModifiedBy` and `ModifiedDate` are null for queries that have not been modified. The combination of a non-null `ModifiedBy` with a null `ModifiedDate` (or vice versa) indicates incomplete metadata and should be treated as a data integrity concern.

This type is not thread-safe for mutation. Once populated by the parser, instances are intended to be treated as immutable snapshots. Concurrent reads are safe; concurrent writes to collection members or properties are not synchronized and will produce undefined behavior. If modification is required, create a copy or use a synchronization mechanism external to the type.
