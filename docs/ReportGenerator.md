# ReportGenerator

The `ReportGenerator` class provides static methods for generating analysis reports in various formats from SQL query data. It is designed to support multiple output types including plain text, CSV, JSON, and HTML, enabling flexible reporting capabilities for database query analysis results.

## API

### GenerateTextReport
Generates a plain text report summarizing SQL query analysis.

**Parameters:** None

**Return Value:** A string containing the formatted text report.

**Exceptions:** Throws `InvalidOperationException` if the analysis data is unavailable or corrupted.

---

### GenerateCsvReport
Generates a CSV-formatted report of SQL query analysis data.

**Parameters:** None

**Return Value:** A string containing the CSV-formatted report.

**Exceptions:** Throws `InvalidOperationException` if the analysis data is unavailable or corrupted.

---

### GenerateJsonReport
Generates a JSON-formatted report of SQL query analysis data.

**Parameters:** None

**Return Value:** A string containing the JSON-formatted report.

**Exceptions:** Throws `InvalidOperationException` if the analysis data is unavailable or corrupted.

---

### GenerateHtmlReport
Generates an HTML-formatted report of SQL query analysis data.

**Parameters:** None

**Return Value:** A string containing the HTML-formatted report.

**Exceptions:** Throws `InvalidOperationException` if the analysis data is unavailable or corrupted.

---

### GenerateSummary
Generates a concise summary of SQL query analysis findings.

**Parameters:** None

**Return Value:** A string containing the summary text.

**Exceptions:** Throws `InvalidOperationException` if the analysis data is unavailable or corrupted.

---

## Usage

```csharp
// Generate and save a text report
string textReport = ReportGenerator.GenerateTextReport();
File.WriteAllText("analysis-report.txt", textReport);
Console.WriteLine("Text report generated successfully.");
```

```csharp
// Generate and output a JSON report
string jsonReport = ReportGenerator.GenerateJsonReport();
using (var client = new HttpClient())
{
    var content = new StringContent(jsonReport, Encoding.UTF8, "application/json");
    await client.PostAsync("https://api.example.com/reports", content);
}
```

---

## Notes

- All methods are static and do not accept parameters. Actual implementations may require internal state or prior configuration to function correctly.
- Thread safety is not guaranteed. Concurrent calls to these methods may produce inconsistent results if they rely on shared mutable state.
- Methods throw `InvalidOperationException` when analysis data is missing or invalid. Ensure prerequisite analysis steps are completed before invocation.
- Output format validity (e.g., CSV structure, JSON schema) depends on the underlying implementation and may require validation in consuming code.
