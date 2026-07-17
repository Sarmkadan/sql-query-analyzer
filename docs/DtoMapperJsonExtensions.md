# DtoMapperJsonExtensions
The `DtoMapperJsonExtensions` class provides a set of extension methods for converting between JSON strings and various DTO (Data Transfer Object) types used in the sql-query-analyzer project. These methods enable easy serialization and deserialization of complex data structures, facilitating communication between different components of the system.

## API
The `DtoMapperJsonExtensions` class offers the following public members:
* `ToJson`: Converts an object to a JSON string.
* `FromJsonToAnalysisRequest`: Attempts to deserialize a JSON string into an `AnalysisRequestDto` object.
* `TryFromJsonToAnalysisRequest`: Attempts to deserialize a JSON string into an `AnalysisRequestDto` object, returning a boolean indicating success.
* `FromJsonToAnalysisResponse`: Attempts to deserialize a JSON string into an `AnalysisResponseDto` object.
* `TryFromJsonToAnalysisResponse`: Attempts to deserialize a JSON string into an `AnalysisResponseDto` object, returning a boolean indicating success.
* `FromJsonToPerformanceIssue`: Attempts to deserialize a JSON string into a `PerformanceIssueDto` object.
* `TryFromJsonToPerformanceIssue`: Attempts to deserialize a JSON string into a `PerformanceIssueDto` object, returning a boolean indicating success.
* `FromJsonToIndexSuggestion`: Attempts to deserialize a JSON string into an `IndexSuggestionDto` object.
* `TryFromJsonToIndexSuggestion`: Attempts to deserialize a JSON string into an `IndexSuggestionDto` object, returning a boolean indicating success.
* `FromJsonToBatchAnalysisRequest`: Attempts to deserialize a JSON string into a `BatchAnalysisRequestDto` object.
* `TryFromJsonToBatchAnalysisRequest`: Attempts to deserialize a JSON string into a `BatchAnalysisRequestDto` object, returning a boolean indicating success.
* `FromJsonToBatchAnalysisResponse`: Attempts to deserialize a JSON string into a `BatchAnalysisResponseDto` object.
* `TryFromJsonToBatchAnalysisResponse`: Attempts to deserialize a JSON string into a `BatchAnalysisResponseDto` object, returning a boolean indicating success.
* `FromJsonToIndexAnalysisRequest`: Attempts to deserialize a JSON string into an `IndexAnalysisRequestDto` object.

## Usage
Here are two examples of using the `DtoMapperJsonExtensions` class:
```csharp
// Example 1: Serializing an AnalysisRequestDto to JSON
var analysisRequest = new AnalysisRequestDto { Query = "SELECT * FROM table" };
var json = analysisRequest.ToJson();
Console.WriteLine(json);

// Example 2: Deserializing a JSON string to an AnalysisResponseDto
var json = "{\"query\":\"SELECT * FROM table\",\"results\":[{\"column\":\"id\",\"value\":1}]}";
var analysisResponse = json.FromJsonToAnalysisResponse();
if (analysisResponse != null)
{
    Console.WriteLine(analysisResponse.Query);
    foreach (var result in analysisResponse.Results)
    {
        Console.WriteLine($"Column: {result.Column}, Value: {result.Value}");
    }
}
```

## Notes
When using the `DtoMapperJsonExtensions` class, keep in mind the following edge cases:
* If the JSON string is invalid or does not match the expected DTO structure, the `FromJsonTo*` methods will return `null`.
* If the `TryFromJsonTo*` methods return `false`, the corresponding DTO object will not be created.
* The `ToJson` method will throw an exception if the object being serialized is `null`.
* The `DtoMapperJsonExtensions` class is thread-safe, as it only uses static methods and does not maintain any internal state. However, the underlying JSON serialization library may have its own thread-safety considerations.
