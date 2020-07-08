# DatabaseConnectionValidatorExtensions

Extension methods for validating SQL database connections and related metadata. These utilities help diagnose connection issues, validate server compatibility, and generate diagnostic reports for SQL Query Analyzer.

## API

### `ValidateConnectionAsync`

Asynchronously validates a database connection using the provided connection string.

- **Parameters**
  - `connectionString` (`string`) – The connection string to validate.
  - `cancellationToken` (`CancellationToken`, optional) – A token to monitor for cancellation requests.

- **Return Value**
  Returns a `Task<ConnectionValidationResult>` representing the validation outcome, including success status and error details.

- **Exceptions**
  Throws `ArgumentNullException` if `connectionString` is `null`.

---

### `ValidateFormatOnlyAsync`

Asynchronously validates only the format of the connection string without establishing a connection.

- **Parameters**
  - `connectionString` (`string`) – The connection string to validate.
  - `cancellationToken` (`CancellationToken`, optional) – A token to monitor for cancellation requests.

- **Return Value**
  Returns a `Task<bool>` indicating whether the connection string format is valid (`true`) or not (`false`).

- **Exceptions**
  Throws `ArgumentNullException` if `connectionString` is `null`.

---

### `GetErrorSummary`

Generates a human-readable summary of validation errors from a `ConnectionValidationResult`.

- **Parameters**
  - `result` (`ConnectionValidationResult`) – The validation result to summarize.

- **Return Value**
  Returns a `string` containing a concise error summary, or an empty string if no errors are present.

- **Exceptions**
  Throws `ArgumentNullException` if `result` is `null`.

---

### `IsConnectionSuccessful`

Determines whether a `ConnectionValidationResult` indicates a successful connection.

- **Parameters**
  - `result` (`ConnectionValidationResult`) – The validation result to check.

- **Return Value**
  Returns `true` if the connection was successful; otherwise, `false`.

- **Exceptions**
  Throws `ArgumentNullException` if `result` is `null`.

---

### `GetFormattedVersion`

Retrieves the formatted server version string from a `ConnectionValidationResult`.

- **Parameters**
  - `result` (`ConnectionValidationResult`) – The validation result containing version information.

- **Return Value**
  Returns a `string` representing the server version (e.g., "Microsoft SQL Server 2022"), or `null` if unavailable.

- **Exceptions**
  Throws `ArgumentNullException` if `result` is `null`.

---
### `GenerateDiagnosticReport`

Generates a detailed diagnostic report from a `ConnectionValidationResult`.

- **Parameters**
  - `result` (`ConnectionValidationResult`) – The validation result to analyze.

- **Return Value**
  Returns a `string` containing a structured diagnostic report with connection details, errors, and version information.

- **Exceptions**
  Throws `ArgumentNullException` if `result` is `null`.

## Usage

### Example 1: Validate a Connection and Generate a Report
