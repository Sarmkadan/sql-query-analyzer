# IConnectionConfiguration

Defines the contract for configuring and managing database connections in SQL Query Analyzer, supporting both SQL Server and PostgreSQL configurations. This interface provides properties for connection parameters and asynchronous methods to validate and initialize database connections.

## API

### `ConnectionString`
- **Purpose**: Gets or sets the full connection string used to establish a database connection.
- **Type**: `string`
- **Remarks**: The exact format depends on the database provider (SQL Server or PostgreSQL). Must be set before calling `TestConnectionAsync` or `InitializeDatabaseAsync`.

### `DatabaseName`
- **Purpose**: Gets or sets the name of the target database.
- **Type**: `string`
- **Remarks**: Used during connection initialization to specify the database context. Must be non-empty when calling `InitializeDatabaseAsync`.

### `ServerName`
- **Purpose**: Gets or sets the name or address of the database server.
- **Type**: `string`
- **Remarks**: Required for establishing a connection. Must be a valid server identifier (e.g., `localhost`, `server.example.com`).

### `CommandTimeout`
- **Purpose**: Gets or sets the time in seconds to wait for a command to execute before terminating the attempt and generating an error.
- **Type**: `int`
- **Default**: Typically `30` seconds if not explicitly set.
- **Remarks**: A value of `0` indicates no timeout (not recommended for production).

### `SqlServerConfiguration`
- **Purpose**: Gets or sets the SQL Server-specific configuration object.
- **Type**: `SqlServerConfiguration`
- **Remarks**: Used to configure SQL Server-specific settings (e.g., authentication mode, encryption). Must be set if targeting SQL Server.

### `PostgresConfiguration`
- **Purpose**: Gets or sets the PostgreSQL-specific configuration object.
- **Type**: `PostgresConfiguration`
- **Remarks**: Used to configure PostgreSQL-specific settings (e.g., SSL mode, connection pooling). Must be set if targeting PostgreSQL.

### `TestConnectionAsync()`
- **Purpose**: Asynchronously verifies whether the configured connection can be established.
- **Returns**: `Task<bool>` – `true` if the connection succeeds; otherwise, `false`.
- **Exceptions**:
  - `InvalidOperationException`: Thrown if required properties (`ServerName`, `DatabaseName`, or `ConnectionString`) are not set.
  - `SqlException`: Thrown if the connection attempt fails due to a SQL Server error.
  - `NpgsqlException`: Thrown if the connection attempt fails due to a PostgreSQL error.
- **Remarks**: Does not modify the database or connection state. Safe to call repeatedly.

### `InitializeDatabaseAsync()`
- **Purpose**: Asynchronously initializes the database by ensuring it exists and is accessible with the configured settings.
- **Returns**: `Task<bool>` – `true` if initialization succeeds; otherwise, `false`.
- **Exceptions**:
  - `InvalidOperationException`: Thrown if required properties (`ServerName`, `DatabaseName`, or `ConnectionString`) are not set.
  - `SqlException`: Thrown if the initialization fails due to a SQL Server error.
  - `NpgsqlException`: Thrown if the initialization fails due to a PostgreSQL error.
- **Remarks**: May create the database if it does not exist. Idempotent; safe to call multiple times.

## Usage

### Example 1: Testing a SQL Server Connection
