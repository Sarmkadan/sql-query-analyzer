# SqlInjectionDetector

The `SqlInjectionDetector` class is a static analysis utility designed to identify potential SQL injection vulnerabilities within SQL query strings. It examines queries for patterns known to be indicative of injection risks, such as improperly parameterized inputs or dynamic SQL construction, and reports findings with contextual details like severity and location.

## API

### `public SqlInjectionDetector`
- **Purpose**: Constructor for the `SqlInjectionDetector` class. Initializes a new instance of the detector.
- **Parameters**: None.
- **Returns**: A new instance of `SqlInjectionDetector`.
- **Throws**: None.

### `public List<SqlInjectionIssue> DetectVulnerabilities`
- **Purpose**: Analyzes the provided SQL query string for potential SQL injection vulnerabilities.
- **Parameters**: None (assumes the query is provided via internal state or another mechanism, as no parameters are exposed in the signature).
- **Returns**: A list of `SqlInjectionIssue` objects, each representing a detected vulnerability. Returns an empty list if no vulnerabilities are found.
- **Throws**: None.

### `public string Type`
- **Purpose**: Gets the type/category of the detected SQL injection issue (e.g., "Tautology", "Union-Based", "Blind").
- **Parameters**: None.
- **Returns**: A string representing the issue type.
- **Throws**: None.

### `public string Severity`
- **Purpose**: Gets the severity level of the detected issue (e.g., "High", "Medium", "Low").
- **Parameters**: None.
- **Returns**: A string representing the severity.
- **Throws**: None.

### `public int Location`
- **Purpose**: Gets the zero-based character position in the query string where the vulnerability was detected.
- **Parameters**: None.
- **Returns**: An integer representing the location.
- **Throws**: None.

### `public string Pattern`
- **Purpose**: Gets the regex pattern or heuristic used to identify the vulnerability.
- **Parameters**: None.
- **Returns**: A string representing the pattern.
- **Throws**: None.

### `public string Description`
- **Purpose**: Gets a human-readable description of the detected vulnerability.
- **Parameters**: None.
- **Returns**: A string describing the issue.
- **Throws**: None.

### `public override string ToString`
- **Purpose**: Returns a formatted string representation of the detected issue, combining `Type`, `Severity`, `Location`, `Pattern`, and `Description`.
- **Parameters**: None.
- **Returns**: A string summarizing the issue details.
- **Throws**: None.

## Usage

### Example 1: Detecting Vulnerabilities in a Query
