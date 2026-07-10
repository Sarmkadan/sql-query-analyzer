# IndexRecommendation

Represents a suggested index improvement for a specific table, containing metadata about the recommendation, its expected impact, and the generated T‑SQL script to create the index.

## API

### RecommendationId  
**Purpose:** Unique identifier for the recommendation, typically a GUID or hash.  
**Parameters:** None.  
**Return value:** The string identifier.  
**Throws:** None.

### TableName  
**Purpose:** Name of the table on which the index should be created.  
**Parameters:** None.  
**Return value:** The table name as a string.  
**Throws:** None.

### KeyColumns  
**Purpose:** Ordered list of column names that form the key of the recommended index.  
**Parameters:** None.  
**Return value:** A `List<string>` containing the key column names.  
**Throws:** None.

### IncludeColumns  
**Purpose:** List of column names to be included as non‑key (included) columns in the index.  
**Parameters:** None.  
**Return value:** A `List<string>` containing the include column names.  
**Throws:** None.

### IndexType  
**Purpose:** Type of index to create (e.g., "NONCLUSTERED", "CLUSTERED", "COLUMNSTORE").  
**Parameters:** None.  
**Return value:** The index type as a string.  
**Throws:** None.

### ImpactScore  
**Purpose:** Relative estimate of the performance benefit if the index is applied; higher values indicate greater expected impact.  
**Parameters:** None.  
**Return value:** A `double` representing the score.  
**Throws:** None.

### Rationale  
**Purpose:** Human‑readable explanation of why the index was recommended.  
**Parameters:** None.  
**Return value:** The rationale as a string.  
**Throws:** None.

### GeneratedScript  
**Purpose:** T‑SQL script that creates the recommended index; populated by `GenerateScript`.  
**Parameters:** None.  
**Return value:** The script as a string, or `null` before generation.  
**Throws:** None.

### Source  
**Purpose:** Origin of the recommendation (e.g., missing index DMV, query plan hint).  
**Parameters:** None.  
**Return value:** A `RecommendationSource` enum value.  
**Throws:** None.

### RecommendedAt  
**Purpose:** Timestamp indicating when the recommendation was produced.  
**Parameters:** None.  
**Return value:** A `DateTime` value.  
**Throws:** None.

### GenerateScript  
**Purpose:** Builds the T‑SQL `CREATE INDEX` statement based on the current property values and stores it in `GeneratedScript`.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:**  
- `InvalidOperationException` if required properties such as `TableName`, `KeyColumns`, or `IndexType` are null or empty.  
- `ArgumentException` if any column name in `KeyColumns` or `IncludeColumns` is null, empty, or whitespace.

## Usage

```csharp
using System;
using System.Collections.Generic;

// Example 1: Creating a recommendation and generating its script
var rec = new IndexRecommendation
{
    RecommendationId = Guid.NewGuid().ToString(),
    TableName = "Orders",
    KeyColumns = new List<string> { "CustomerID", "OrderDate" },
    IncludeColumns = new List<string> { "TotalAmount" },
    IndexType = "NONCLUSTERED",
    ImpactScore = 8.5,
    Rationale = "Frequent filters on CustomerID and OrderDate with retrieval of TotalAmount.",
    Source = RecommendationSource.DmvMissingIndex,
    RecommendedAt = DateTime.UtcNow
};

rec.GenerateScript(); // populates GeneratedScript
Console.WriteLine(rec.GeneratedScript);
```

```csharp
using System;
using System.Collections.Generic;

// Example 2: Inspecting a recommendation before script generation
var rec = GetRecommendationFromAnalyzer(); // assumed to return a populated IndexRecommendation

if (rec.ImpactScore > 7.0 && rec.KeyColumns.Count > 0)
{
    Console.WriteLine($"High‑impact index suggested for {rec.TableName}");
    Console.WriteLine($"Key columns: {string.Join(", ", rec.KeyColumns)}");
    if (rec.IncludeColumns.Count > 0)
    {
        Console.WriteLine($"Include columns: {string.Join(", ", rec.IncludeColumns)}");
    }
    // Generate script only when needed
    if (string.IsNullOrEmpty(rec.GeneratedScript))
    {
        rec.GenerateScript();
    }
    Console.WriteLine($"Script: {rec.GeneratedScript}");
}
```

## Notes

- The `KeyColumns` and `IncludeColumns` lists are mutable; altering them after `GenerateScript` has been called will not automatically update `GeneratedScript`. Call `GenerateScript` again to reflect changes.  
- `ImpactScore` is not bounded by the type; callers should treat values outside the expected range (e.g., negative) as invalid.  
- The class does not implement any synchronization mechanisms. Concurrent read access to its properties is safe if the instance is not being modified, but concurrent writes (including calls to `GenerateScript`) require external locking.  
- Passing null or empty lists for `KeyColumns` will cause `GenerateScript` to throw; the constructor does not validate these fields.  
- `GeneratedScript` remains `null` until `GenerateScript` is successfully invoked; subsequent calls will overwrite the previous script.  
- The `Source` enum is assumed to be defined elsewhere; using an undefined value will not cause an exception but may be treated as unknown by consumers.
