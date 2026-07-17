# PerformanceIssueExtensions

Provides a set of static extension methods for working with `PerformanceIssue` objects, enabling filtering, grouping, ordering, and metadata extraction for analysis and reporting scenarios.

## API

### `DeepCopy`
Creates a deep copy of a `PerformanceIssue` instance, ensuring all nested properties and collections are duplicated.

- **Parameters**
  - `issue` (`PerformanceIssue`): The issue to copy.
- **Returns**
  - (`PerformanceIssue`): A new `PerformanceIssue` instance with copied data.
- **Throws**
  - `ArgumentNullException`: If `issue` is `null`.

---

### `GetImpactDescription`
Returns a human-readable description of the performance impact severity for a given issue.

- **Parameters**
  - `issue` (`PerformanceIssue`): The issue to evaluate.
- **Returns**
  - (`string`): A localized description of the impact severity (e.g., "High", "Medium", "Low").
- **Throws**
  - `ArgumentNullException`: If `issue` is `null`.

---

### `GetTimeIncreaseDescription`
Returns a description of the estimated time increase caused by the issue, if available.

- **Parameters**
  - `issue` (`PerformanceIssue`): The issue to evaluate.
- **Returns**
  - (`string?`): A description of the time increase (e.g., "~2x slower") or `null` if not applicable.
- **Throws**
  - `ArgumentNullException`: If `issue` is `null`.

---
### `GetLocationInfo`
Returns a string representing the location of the issue within the query or execution plan.

- **Parameters**
  - `issue` (`PerformanceIssue`): The issue to locate.
- **Returns**
  - (`string`): A string describing the location (e.g., "Table Scan on `Orders`", "Index Seek on `idx_CustomerID`").
- **Throws**
  - `ArgumentNullException`: If `issue` is `null`.

---
### `GetMetadataPairs`
Retrieves a collection of key-value pairs representing metadata associated with the issue.

- **Parameters**
  - `issue` (`PerformanceIssue`): The issue to inspect.
- **Returns**
  - (`IEnumerable<KeyValuePair<string, string>>`): An enumerable of metadata entries (e.g., `{"RowsExamined", "12500"}, {"DurationMs", "450"}`).
- **Throws**
  - `ArgumentNullException`: If `issue` is `null`.

---
### `IsActionable`
Determines whether the issue is actionable, i.e., whether it suggests a potential optimization.

- **Parameters**
  - `issue` (`PerformanceIssue`): The issue to evaluate.
- **Returns**
  - (`bool`): `true` if the issue is actionable; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `issue` is `null`.

---
### `GetIssueTypeLabel`
Returns a localized label describing the type of the issue.

- **Parameters**
  - `issue` (`PerformanceIssue`): The issue to classify.
- **Returns**
  - (`string`): A label describing the issue type (e.g., "Missing Index", "Cardinality Estimate").
- **Throws**
  - `ArgumentNullException`: If `issue` is `null`.

---
### `GetPriorityLabel`
Returns a localized label indicating the priority of the issue for remediation.

- **Parameters**
  - `issue` (`PerformanceIssue`): The issue to prioritize.
- **Returns**
  - (`string`): A priority label (e.g., "High", "Medium", "Low").
- **Throws**
  - `ArgumentNullException`: If `issue` is `null`.

---
### `FilterBySeverity`
Filters a sequence of `PerformanceIssue` instances by their severity level.

- **Parameters**
  - `issues` (`IEnumerable<PerformanceIssue>`): The sequence of issues to filter.
  - `severity` (`Severity`): The minimum severity level to include.
- **Returns**
  - (`IEnumerable<PerformanceIssue>`): A filtered sequence containing only issues with severity ≥ `severity`.
- **Throws**
  - `ArgumentNullException`: If `issues` is `null`.

---
### `FilterByImpact`
Filters a sequence of `PerformanceIssue` instances by their impact level.

- **Parameters**
  - `issues` (`IEnumerable<PerformanceIssue>`): The sequence of issues to filter.
  - `impact` (`Impact`): The minimum impact level to include.
- **Returns**
  - (`IEnumerable<PerformanceIssue>`): A filtered sequence containing only issues with impact ≥ `impact`.
- **Throws**
  - `ArgumentNullException`: If `issues` is `null`.

---
### `OrderByPriority`
Orders a sequence of `PerformanceIssue` instances by priority (descending), then by impact (descending).

- **Parameters**
  - `issues` (`IEnumerable<PerformanceIssue>`): The sequence of issues to order.
- **Returns**
  - (`IOrderedEnumerable<PerformanceIssue>`): An ordered sequence sorted by priority and impact.
- **Throws**
  - `ArgumentNullException`: If `issues` is `null`.

---
### `GroupByIssueType`
Groups a sequence of `PerformanceIssue` instances by their issue type.

- **Parameters**
  - `issues` (`IEnumerable<PerformanceIssue>`): The sequence of issues to group.
- **Returns**
  - (`IReadOnlyDictionary<IssueType, IReadOnlyList<PerformanceIssue>>`): A dictionary mapping each `IssueType` to a list of corresponding issues.
- **Throws**
  - `ArgumentNullException`: If `issues` is `null`.

## Usage
