# PlanVisualizationValidation

Provides validation helpers for `PlanVisualization` instances.

## Purpose

The `PlanVisualizationValidation` static class contains extension methods for validating `PlanVisualization` objects. It checks for required fields, data integrity, and business rules, returning human-readable validation messages or throwing exceptions when validation fails.

## Members

### Validate(this PlanVisualization? value)

Validates a `PlanVisualization` instance and returns a list of human-readable problems.

- **Parameters**
  - `value`: The plan visualization to validate.
- **Returns**
  - An `IReadOnlyList<string>` containing validation messages; empty if the instance is valid.
- **Exceptions**
  - `ArgumentNullException`: Thrown when `value` is null.

### IsValid(this PlanVisualization? value)

Determines whether a `PlanVisualization` instance is valid.

- **Parameters**
  - `value`: The plan visualization to check.
- **Returns**
  - `true` if valid; otherwise, `false`.
- **Exceptions**
  - `ArgumentNullException`: Thrown when `value` is null.

### EnsureValid(this PlanVisualization? value)

Ensures that a `PlanVisualization` instance is valid, throwing an `ArgumentException` with a detailed message listing all validation failures if it is not.

- **Parameters**
  - `value`: The plan visualization to validate.
- **Exceptions**
  - `ArgumentNullException`: Thrown when `value` is null.
  - `ArgumentException`: Thrown when the plan visualization is invalid, with a message listing all problems.

## Validation Rules

The validation checks the following:

1. **TextTree**: Cannot be null, empty, or whitespace.
2. **CostDistribution**: Cannot be null, empty, or whitespace.
3. **Bottlenecks**:
   - Cannot be null.
   - Each bottleneck must have:
     - Non-null, non-empty, non-whitespace `NodeId`.
     - Non-null, non-empty, non-whitespace `NodeType`.
     - Non-null, non-empty, non-whitespace `ObjectName`.
     - Non-negative `EstimatedCost`.
     - Non-negative `Depth`.
     - Non-null, non-empty, non-whitespace `Recommendation`.
4. **Stats**:
   - Cannot be null.
   - Cannot be empty.
5. **RenderedAt**: Cannot be the default `DateTime` value (i.e., `DateTime.MinValue`).

## Example

```csharp
using SqlQueryAnalyzer.Models;

// Create a plan visualization to validate
var plan = new PlanVisualization
{
    TextTree = "|--Index Seek(OBJECT: ([table]))",
    CostDistribution = "100%",
    Bottlenecks = new List<Bottleneck>
    {
        new Bottleneck
        {
            NodeId = "1",
            NodeType = "Index Seek",
            ObjectName = "table",
            EstimatedCost = 0.5,
            Depth = 0,
            Recommendation = "Consider adding an index"
        }
    },
    Stats = new Dictionary<string, string>
    {
        {"EstimatedRows", "100"}
    },
    RenderedAt = DateTime.UtcNow
};

// Validate the plan
var errors = plan.Validate();
if (errors.Count == 0)
{
    Console.WriteLine("Plan visualization is valid.");
}
else
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}

// Alternatively, use IsValid
if (plan.IsValid())
{
    Console.WriteLine("Plan visualization is valid.");
}

// Or use EnsureValid (throws exception on invalid)
try
{
    plan.EnsureValid();
    Console.WriteLine("Plan visualization is valid.");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```