# CommandLineParserJsonExtensions

## Purpose
Provides JSON serialization and deserialization extensions for CommandLineArguments, enabling conversion to and from JSON format with configurable options.

## Members

### ToJson(value, indented=false)
Converts a CommandLineArguments instance to a JSON string.

- **Parameters**:
  - `value`: The command line arguments to serialize.
  - `indented`: Optional. Whether to format the JSON with indentation for readability. Defaults to `false`.
- **Returns**: A JSON string representation of the command line arguments.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `value` is null.

### FromJson(json)
Parses a JSON string into a CommandLineArguments instance.

- **Parameters**:
  - `json`: The JSON string to deserialize.
- **Returns**: The deserialized CommandLineArguments instance if successful; otherwise, `null`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `json` is null.
  - `JsonException`: Thrown when the JSON is invalid or cannot be deserialized.

### TryFromJson(json, out value)
Attempts to parse a JSON string into a CommandLineArguments instance without throwing exceptions.

- **Parameters**:
  - `json`: The JSON string to deserialize.
  - `value`: Receives the deserialized CommandLineArguments if successful; otherwise, `null`.
- **Returns**: `true` if deserialization succeeded; otherwise, `false`.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `json` is null.

## Usage Example
```csharp
using SqlQueryAnalyzer.CLI;

// Create command line arguments
var args = new CommandLineArguments
{
    Query = "SELECT * FROM Users",
    OutputFormat = "json",
    Verbose = true,
    ThreadCount = 4
};

// Serialize to JSON (compact)
string json = args.ToJson();

// Serialize to JSON (indented)
string indentedJson = args.ToJson(indented: true);

// Deserialize from JSON
CommandLineArguments? parsedArgs = CommandLineParserJsonExtensions.FromJson(json);
if (parsedArgs != null)
{
    Console.WriteLine($"Deserialized query: {parsedArgs.Query}");
    Console.WriteLine($"Verbose: {parsedArgs.Verbose}");
}

// Try deserialization (safe)
if (CommandLineParserJsonExtensions.TryFromJson(json, out var tryParsedArgs))
{
    if (tryParsedArgs != null)
    {
        Console.WriteLine($"Try deserialized thread count: {tryParsedArgs.ThreadCount}");
    }
}
```