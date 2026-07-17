# AnalysisControllerValidation

`AnalysisControllerValidation` is a static utility class responsible for validating SQL query analysis controllers and their configurations. It provides methods to check the validity of controllers, retrieve validation error messages, and enforce validation constraints through exceptions. This class is part of the `sql-query-analyzer` project and serves as a centralized validation mechanism for controller components.

## API

### `Validate`

```csharp
public static IReadOnlyList<string> Validate(AnalysisController controller)
```

Validates the provided `AnalysisController` instance and returns a list of error messages.  
**Parameters**:  
- `controller`: The `AnalysisController` to validate.  
**Returns**:  
- `IReadOnlyList<string>`: A list of validation error messages. Returns an empty list if valid.  
**Throws**:  
- `ArgumentNullException`: If `controller` is `null`.

---

### `Validate<T>`

```csharp
public static IReadOnlyList<string> Validate<T>(T controller) where T : AnalysisController
```

Validates a generic `AnalysisController` derived type and returns error messages.  
**Parameters**:  
- `controller`: The controller instance of type `T` to validate.  
**Returns**:  
- `IReadOnlyList<string>`: A list of validation error messages.  
**Throws**:  
- `ArgumentNullException`: If `controller` is `null`.

---

### `IsValid`

```csharp
public static bool IsValid(AnalysisController controller)
```

Checks whether the provided `AnalysisController` is valid.  
**Parameters**:  
- `controller`: The `AnalysisController` to validate.  
**Returns**:  
- `bool`: `true` if valid; `false` otherwise.  
**Throws**:  
- `ArgumentNullException`: If `controller` is `null`.

---

### `IsValid<T>`

```csharp
public static bool IsValid<T>(T controller) where T : AnalysisController
```

Checks whether a generic `AnalysisController` derived type is valid.  
**Parameters**:  
- `controller`: The controller instance of type `T` to validate.  
**Returns**:  
- `bool`: `true` if valid; `false` otherwise.  
**Throws**:  
- `ArgumentNullException`: If `controller` is `null`.

---

### `EnsureValid`

```csharp
public static void EnsureValid(AnalysisController controller)
```

Validates the provided `AnalysisController` and throws an exception if invalid.  
**Parameters**:  
- `controller`: The `AnalysisController` to validate.  
**Throws**:  
- `ArgumentNullException`: If `controller` is `null`.  
- `InvalidOperationException`: If validation fails.

---

### `EnsureValid<T>`

```csharp
public static void EnsureValid<T>(T controller) where T : AnalysisController
```

Validates a generic `AnalysisController` derived type and throws an exception if invalid.  
**Parameters**:  
- `controller`: The controller instance of type `T` to validate.  
**Throws**:  
- `ArgumentNullException`: If `controller` is `null`.  
- `InvalidOperationException`: If validation fails.

---

## Usage

### Example 1: Validating a Controller

```csharp
var controller = new CustomAnalysisController();
var errors = AnalysisControllerValidation.Validate(controller);

if (errors.Any())
{
    Console.WriteLine($"Validation failed: {string.Join(", ", errors)}");
}
else
{
    Console.WriteLine("Controller is valid.");
}
```

### Example 2: Enforcing Validation with Exception Handling

```csharp
try
{
    AnalysisControllerValidation.EnsureValid(myController);
    // Proceed with controller usage
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Validation error: {ex.Message}");
}
```

## Notes

- **Null Handling**: All methods throw `ArgumentNullException` if the input controller is `null`.
- **Thread Safety**: Methods are static and stateless; they are thread-safe provided the underlying `AnalysisController` implementations are thread-safe.
- **Generic Constraints**: The generic `Validate<T>` and `IsValid<T>` methods require `T` to inherit from `AnalysisController`.
- **Error Messages**: `Validate` methods return detailed error messages for diagnostic purposes, while `IsValid` provides a boolean result for quick checks.
- **Exception Behavior**: `EnsureValid` methods are designed for fail-fast scenarios, throwing `InvalidOperationException` immediately upon validation failure.
