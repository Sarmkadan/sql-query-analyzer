# CommandLineArgumentsJsonExtensions

## Purpose
Provides JSON serialization and deserialization extensions for the `CommandLineArguments` class, enabling conversion to and from JSON format with configurable options.

## Members
### ToJson(value, indented=false)
Converts a `CommandLineArguments` instance to a JSON string.

- **Parameters**:
  - `value`: The command line arguments to serialize.
  - `indented`: Optional. Whether to format the JSON with indentation for readability. Defaults to `false`.
- **Returns**: A JSON string representation of the command line arguments.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `value` is null.

### FromJson(json)
Parses a JSON string into a `CommandLineArguments` instance.

- **Parameters**:
  - `json`: The JSON string to deserialize.
- **Returns**: The deserialized `CommandLineArguments` instance if successful; otherwise, `null`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `json` is null.
  - `JsonException`: Thrown when the JSON is invalid or cannot be deserialized.

### TryFromJson(json, out value)
Attempts to parse a JSON string into a `CommandLineArguments` instance without throwing exceptions.

- **Parameters**:
  - `json`: The JSON string to deserialize.
  - `value`: Receives the deserialized `CommandLineArguments` if successful; otherwise, `null`.
- **Returns**: `true` if deserialization succeeded; otherwise, `false`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `json` is null.

## Usage Example
```csharp
using SqlQueryAnalyzer.CLI;

// Create command line arguments
var args = new CommandLineArguments
{
    ConnectionString = "Server=localhost;Database=test;User Id=sa;Password=your_password;",
    Query = "SELECT * FROM Users",
    Format = OutputFormat.Json
};

// Serialize to JSON (compact)
string json = args.ToJson();
// Serialize to JSON (indented)
string indentedJson = args.ToJson(indented: true);

// Deserialize from JSON
CommandLineArguments? parsedArgs = CommandLineArgumentsJsonExtensions.FromJson(json);
if (parsedArgs != null)
{
    Console.WriteLine($"Deserialized query: {parsedArgs.Query}");
}

// Try deserialization (safe)
if (CommandLineArgumentsJsonExtensions.TryFromJson(json, out var tryParsedArgs))
{
    if (tryParsedArgs != null)
    {
        Console.WriteLine($"Try deserialized format: {tryParsedArgs.Format}");
    }
}
```