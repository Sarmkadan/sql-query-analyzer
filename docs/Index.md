# Index
Represents metadata about a database index as discovered by the SQL Query Analyzer. The type aggregates structural, usage, and storage characteristics of an index so that callers can compare, report, or make tuning decisions based on the collected information.

## API
| Member | Type | Purpose | Remarks |
|--------|------|---------|---------|
| `IndexId` | `string` | Unique identifier for the index within its database (often a GUID or composite key). | Read‑only; never null after construction. |
| `IndexName` | `string` | The name of the index as defined in the database. | Read‑only; may be empty string if the source metadata omitted a name. |
| `TableName` | `string` | Name of the table (or view) on which the index is defined. | Read‑only; never null. |
| `SchemaName` | `string` | Schema that owns the table containing the index. | Read‑only; may be null for databases without schema qualification. |
| `IndexType` | `IndexType` | Enumerated value describing the index kind (e.g., Clustered, NonClustered, XML, Spatial). | Read‑ | Read‑only; always a valid `IndexType` value. |
| `IsUnique` | `bool` | Indicates whether the index enforces uniqueness of its key values. | Read‑only; false for non‑unique indexes. |
| `IsPrimaryKey` | `bool` | Indicates whether the index backs a PRIMARY KEY constraint. | Read‑only; mutually exclusive with `IsUnique` only when the primary key is not unique (which cannot occur). |
| `IsDisabled` | `bool` | Indicates whether the index is currently disabled and not usable by the query optimizer. | Read‑only; true when the index has been explicitly disabled via `ALTER INDEX … DISABLE`. |
| `IsFiltered` | `bool` | Indicates whether the index has a filter predicate applied. | Read‑only; true when `FilterPredicate` is non‑null. |
| `Columns`Columns` | `List<IndexColumn> of key columns. The order reflects the index key ordering. | Read‑only; list may be empty for certain special indexes (e.g., some XML indexes). Elements are never null. |
| `IncludeColumns` | `List<string>` | List of column names included in the index as non‑key (included) columns. | Read‑only; may be empty or null if the source did not provide include information. |
| `SizeInBytes` | `long` | Approximate storage size of the index, in bytes. | Read‑only; zero if size information unavailable. |
| `PageCount` | `int` | Number of data pages allocated to the index. | Read‑only; zero when page count unknown. |
| `FileGroup` | `string` | Name of the filegroup on which the index resides. | Read‑only; may be null for indexes not bound to a specific filegroup (e.g., partitioned indexes using multiple filegroups). |
| `FilterPredicate` | `string?` | The filter expression for a filtered index, or null if the index is not filtered. | Read‑only; null when `IsFiltered` is false. |
| `UserSeeks` | `long` | Cumulative count of seek operations performed by user queries on this index since the last statistics reset. | Read‑only; monotonically non‑decreasing. |
| `UserScans` | `long` | Cumulative count of scan operations performed by user queries on this index. | Read‑only; monotonically non‑decreasing. |
| `UserLookups` | `long` | Cumulative count bookmark lookups (key lookups) performed by user queries on this index since the last statistics reset. Read‑only; monotonically non‑decreasing. |
| `UserLookups` | `long` | Cumulative count of lookup operations (bookmark lookups) performed by user queries on this index. | Read‑only; monotonically non‑decreasing. |
| `UserUpdates` | `long` | Cumulative count of update, insert, and delete operations that modified the index. | Read‑only; monotonically non‑decreasing. |
| `LastUserSeekTime` | `long` | Timestamp (in UTC ticks) of the most recent user seek operation on the index. | Read‑only; zero if no seeks have been recorded. |

All members are simple get‑only properties or fields; they do not accept parameters and do not throw exceptions under normal usage. Invalid or missing source data results in default values (e.g., empty strings, zero, null) rather than exceptions.

## Usage
```csharp
// Example 1: Print a brief summary of each index retrieved from a database.
foreach (var idx in analyzer.GetIndexes(connectionString))
{
    Console.WriteLine(
        $"{idx.SchemaName}.{idx.TableName}.{idx.IndexName} " +
        $"({idx.IndexType}) Size:{idx.SizeInBytes/1024}KB " +
        $"Seeks:{idx.UserSeeks} Scans:{idx.UserScans}");
}

// Example 2: Identify unused non‑clustered indexes that are candidates for removal.
var unused = analyzer.GetIndexes(connectionString)
    .Where(i => i.IndexType == IndexType.NonClustered &&
                !i.IsUnique &&
                !i.IsPrimaryKey &&
                i.UserSeeks == 0 &&
                i.UserScans == 0 &&
                i.UserLookups == 0);

foreach (var idx in unused)
{
    Console.WriteLine(
        $"Consider dropping {idx.SchemaName}.{idx.TableName}.{idx.IndexName} " +
        $" (last seek: {idx.LastUserSeekTime})");
}
```

## Notes
- The properties reflect a snapshot taken at the time the `Index` object was created; they do not auto‑refresh if the underlying database changes.
- `IndexId` is intended to be globally unique within the database, but callers should not rely on its format; treat it as an opaque identifier.
- `FilterPredicate` may contain arbitrary SQL; consumers should not attempt to execute it directly without proper validation.
- Because all members are immutable after construction, the type is inherently thread‑safe for concurrent read access. No locking is required when sharing `Index` instances across threads.
- If the source metadata does not provide a value for a column (e.g., `IncludeColumns`), the property will be set to `null` or an empty collection as indicated; callers should guard against null when iterating. 
- Numeric counters (`UserSeeks`, `UserScans`, `UserLookups`, `UserUpdates`, `LastUserSeekTime`) are cumulative since the last statistics reset; they wrap only if the underlying SQL Server counter overflows, which is extremely unlikely in practice. 
- The `IndexType` enumeration is defined elsewhere in the codebase; adding new enum values does not affect the contract of this type.
