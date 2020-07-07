# SlowQueryEntryExtensions

Static extension class that provides formatting and evaluation helpers for `SlowQueryEntry` instances, allowing concise, readable representation of query performance data.

## API

### FormatDuration
```csharp
public static string FormatDuration(this SlowQueryEntry entry)
```
**Purpose:** Returns a human‑readable string that represents the query’s execution duration (e.g., `"2.35s"`).  
**Parameters:** `entry` – the `SlowQueryEntry` instance to format.  
**Return value:** A formatted duration string.  
**Exceptions:** Throws `ArgumentNullException` if `entry` is `null`.

### FormatLockTime
```csharp
public static string FormatLockTime(this SlowQueryEntry entry)
```
**Purpose:** Returns a human‑readable string that represents the time the query spent waiting for locks (e.g., `"120ms"`).  
**Parameters:** `entry` – the `SlowQueryEntry` instance to format.  
**Return value:** A formatted lock‑time string.  
**Exceptions:** Throws `ArgumentNullException` if `entry` is `null`.

### IsSlow
```csharp
public static bool IsSlow(this SlowQueryEntry entry)
```
**Purpose:** Determines whether the query exceeds the configured slow‑query threshold.  
**Parameters:** `entry` – the `SlowQueryEntry` instance to evaluate.  
**Return value:** `true` if the query’s duration is greater than the threshold; otherwise `false`.  
**Exceptions:** Throws `ArgumentNullException` if `entry` is `null`.

### GetEfficiencyScore
```csharp
public static string GetEfficiencyScore(this SlowQueryEntry entry)
```
**Purpose:** Returns a string representing an efficiency score derived from the query’s duration, lock time, and rows examined (format defined by the implementation, e.g., `"78%"`).  
**Parameters:** `entry` – the `SlowQueryEntry` instance to score.  
**Return value:** An efficiency score as a string.  
**Exceptions:** Throws `ArgumentNullException` if `entry` is `null`.

## Usage

```csharp
var entry = repository.GetSlowQuery(42);

// Format duration and lock time for logging
string logMessage = $"Query {entry.Id} took {entry.FormatDuration()} " +
                    f"with lock time {entry.FormatLockTime()}.";

// Determine if the query warrants investigation
if (entry.IsSlow())
{
    Console.WriteLine($"Slow query detected: {entry.GetEfficiencyScore()} efficiency");
}
```

```csharp
// Using the extensions in a LINQ projection
var slowQueries = context.SlowQueries
    .Where(q => q.IsSlow())
    .Select(q => new
    {
        q.Id,
        Duration = q.FormatDuration(),
        LockTime = q.FormatLockTime(),
        Score = q.GetEfficiencyScore()
    })
    .ToList();
```

## Notes
- All extension methods are **null‑safe** only insofar as they explicitly check for a `null` `entry` and throw `ArgumentNullException`; they do not return default values for null inputs.
- The methods rely solely on the data contained within the supplied `SlowQueryEntry` instance; they access no static or shared state, making them **thread‑safe** for concurrent invocation on different instances.
- Formatting is culture‑invariant; the output strings use the invariant culture’s number formatting to ensure consistency across environments.
- If the underlying `SlowQueryEntry` properties contain unexpected values (e.g., negative durations), the methods will still produce a formatted string based on those values; callers should validate entry data upstream if such cases are undesirable.
