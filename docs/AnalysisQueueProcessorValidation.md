# AnalysisQueueProcessorValidation

The `AnalysisQueueProcessorValidation` class provides validation helpers for `AnalysisTask` instances within the SQL Query Analyzer background processing system. It validates all public members of an `AnalysisTask` according to their semantic meaning and constraints, ensuring data integrity throughout the task lifecycle.

## API

### Validate
```csharp
public static IReadOnlyList<string> Validate(this AnalysisTask? value)
```
Validates the specified `AnalysisTask` instance and returns a list of validation problems (empty if valid).

*   **Parameters:**
    *   `value`: The `AnalysisTask` instance to validate.
*   **Return Value**: An `IReadOnlyList<string>` containing validation error descriptions. If the task is valid, the list is empty.
*   **Throws**: `ArgumentNullException` if `value` is null.

### IsValid
```csharp
public static bool IsValid(this AnalysisTask? value)
```
Determines whether the specified `AnalysisTask` instance is valid.

*   **Parameters:**
    *   `value`: The `AnalysisTask` instance to check.
*   **Return Value**: `true` if the instance is valid; otherwise, `false`.
*   **Throws**: `ArgumentNullException` if `value` is null.

### EnsureValid
```csharp
public static void EnsureValid(this AnalysisTask? value)
```
Ensures that the specified `AnalysisTask` instance is valid, throwing an exception if validation fails.

*   **Parameters:**
    *   `value`: The `AnalysisTask` instance to validate.
*   **Return Value**: None.
*   **Throws**: 
    *   `ArgumentNullException` if `value` is null.
    *   `ArgumentException` if the instance is not valid, containing details of all validation problems.

## Usage

### Example 1: Pre-processing Validation
Use `IsValid` to check task validity before processing to avoid unnecessary work on invalid data.

```csharp
public void ProcessTask(AnalysisTask task)
{
    if (!task.IsValid())
    {
        var errors = task.Validate();
        logger.LogWarning("Skipping invalid task {TaskId}: {Errors}", 
            task.TaskId, string.Join("; ", errors));
        return;
    }
    
    // Proceed with valid task
    ExecuteAnalysis(task);
}
```

### Example 2: Enforcing Data Integrity
Use `EnsureValid` at critical points to guarantee task validity, allowing invalid states to bubble up as exceptions immediately.

```csharp
public void SaveTaskToDatabase(AnalysisTask task)
{
    // Throws immediately if task is invalid
    task.EnsureValid();
    
    // Proceed with confidence that the task is valid
    _taskRepository.Save(task);
}
```

### Example 3: Collecting Validation Feedback
Use `Validate` to collect all validation errors for user feedback or logging.

```csharp
public ValidationResult TryValidateTask(AnalysisTask task)
{
    var errors = task.Validate();
    return errors.Count == 0 
        ? ValidationResult.Success() 
        : ValidationResult.Failure(errors);
}
```

## Notes

*   **Extension Method Pattern**: All methods are implemented as extension methods on `AnalysisTask?`, allowing fluent usage like `task.Validate()`, `task.IsValid()`, and `task.EnsureValid()`.
*   **Null Safety**: All methods properly handle null inputs by throwing `ArgumentNullException` before attempting validation.
*   **Comprehensive Validation**: The validation covers:
    *   Required string fields (`TaskId`, `Query`) - cannot be null, empty, or whitespace
    *   DateTime constraints - `CreatedAt` cannot be default or too far in future
    *   Temporal consistency - `StartedAt` and `CompletedAt` must be after `CreatedAt` and in logical order
    *   Status-dependent validation - `Result` must be set for `Completed` status, `ErrorMessage` must be set for `Failed` status
    *   Cross-field validation - ensuring time values are chronologically consistent
*   **Error Aggregation**: The `Validate` method returns a list, implying that multiple validation errors can be detected and reported simultaneously rather than failing on the first encounter. Consumers should iterate the full list to present comprehensive feedback.
*   **Thread Safety**: The validation methods are stateless and thread-safe, operating only on the provided instance data.