#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;

namespace SqlQueryAnalyzer.DTOs;

/// <summary>
/// Request DTO for analyzing a query
/// </summary>
public sealed class AnalysisRequestDto
{
    /// <summary>
    /// Gets or sets the query text.
    /// </summary>
    public string QueryText { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the application name.
    /// </summary>
    public string? ApplicationName { get; set; }
    /// <summary>
    /// Gets or sets the procedure name.
    /// </summary>
    public string? ProcedureName { get; set; }
    /// <summary>
    /// Gets or sets the module name.
    /// </summary>
    public string? ModuleName { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether include index suggestions.
    /// </summary>
    public bool IncludeIndexSuggestions { get; set; } = true;
    /// <summary>
    /// Gets or sets a value indicating whether analyze fragmentation.
    /// </summary>
    public bool AnalyzeFragmentation { get; set; } = true;
    /// <summary>
    /// Gets or sets a value indicating whether analyze plan.
    /// </summary>
    public bool AnalyzePlan { get; set; } = false;
    /// <summary>
    /// Gets or sets the execution plan XML.
    /// </summary>
    public string? ExecutionPlanXml { get; set; }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    public override string ToString() =>
        $"AnalysisRequestDto {{ QueryText = {QueryText}, ApplicationName = {ApplicationName}, ProcedureName = {ProcedureName}, ModuleName = {ModuleName}, IncludeIndexSuggestions = {IncludeIndexSuggestions}, AnalyzeFragmentation = {AnalyzeFragmentation} }}";
}

/// <summary>
/// Response DTO for analysis results
/// </summary>
public sealed class AnalysisResponseDto
{
    /// <summary>
    /// Gets or sets the query ID.
    /// </summary>
    public string QueryId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the performance score.
    /// </summary>
    public double PerformanceScore { get; set; }
    /// <summary>
    /// Gets or sets the complexity level.
    /// </summary>
    public string ComplexityLevel { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the issue count.
    /// </summary>
    public int IssueCount { get; set; }
    /// <summary>
    /// Gets or sets the critical issue count.
    /// </summary>
    public int CriticalIssueCount { get; set; }
    /// <summary>
    /// Gets or sets the issues.
    /// </summary>
    public List<PerformanceIssueDto> Issues { get; set; } = [];
    /// <summary>
    /// Gets or sets the index suggestions.
    /// </summary>
    public List<IndexSuggestionDto> IndexSuggestions { get; set; } = [];
    /// <summary>
    /// Gets or sets the summary.
    /// </summary>
    public string Summary { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the analysis time ms.
    /// </summary>
    public long AnalysisTimeMs { get; set; }
}

/// <summary>
/// DTO for performance issue
/// </summary>
public sealed class PerformanceIssueDto
{
    /// <summary>
    /// Gets or sets the issue type.
    /// </summary>
    public string IssueType { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the severity.
    /// </summary>
    public string Severity { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the estimated impact.
    /// </summary>
    public double EstimatedImpact { get; set; }
    /// <summary>
    /// Gets or sets the recommended fix.
    /// </summary>
    public string RecommendedFix { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the priority.
    /// </summary>
    public int Priority { get; set; }
}

/// <summary>
/// DTO for index suggestion
/// </summary>
public sealed class IndexSuggestionDto
{
    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string TableName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the index name.
    /// </summary>
    public string IndexName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the columns.
    /// </summary>
    public List<string> Columns { get; set; } = [];
    /// <summary>
    /// Gets or sets the include columns.
    /// </summary>
    public List<string> IncludeColumns { get; set; } = [];
    /// <summary>
    /// Gets or sets the estimated gain.
    /// </summary>
    public double EstimatedGain { get; set; }
    /// <summary>
    /// Gets or sets the create script.
    /// </summary>
    public string CreateScript { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the affected queries.
    /// </summary>
    public int AffectedQueries { get; set; }
}

/// <summary>
/// DTO for batch analysis request
/// </summary>
public sealed class BatchAnalysisRequestDto
{
    /// <summary>
    /// Gets or sets the queries.
    /// </summary>
    public List<string> Queries { get; set; } = [];
    /// <summary>
    /// Gets or sets the application name.
    /// </summary>
    public string? ApplicationName { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether analyze patterns.
    /// </summary>
    public bool AnalyzePatterns { get; set; } = true;
    /// <summary>
    /// Gets or sets the timeout seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 300;
}

/// <summary>
/// DTO for batch analysis response
/// </summary>
public sealed class BatchAnalysisResponseDto
{
    /// <summary>
    /// Gets or sets the total queries.
    /// </summary>
    public int TotalQueries { get; set; }
    /// <summary>
    /// Gets or sets the successful analyses.
    /// </summary>
    public int SuccessfulAnalyses { get; set; }
    /// <summary>
    /// Gets or sets the failed analyses.
    /// </summary>
    public int FailedAnalyses { get; set; }
    /// <summary>
    /// Gets or sets the results.
    /// </summary>
    public List<AnalysisResponseDto> Results { get; set; } = [];
    /// <summary>
    /// Gets or sets the plus one patterns.
    /// </summary>
    public List<string> NPlusOnePatterns { get; set; } = [];
    /// <summary>
    /// Gets or sets the average score.
    /// </summary>
    public double AverageScore { get; set; }
    /// <summary>
    /// Gets or sets the total analysis time ms.
    /// </summary>
    public long TotalAnalysisTimeMs { get; set; }
}

/// <summary>
/// DTO for index analysis request
/// </summary>
public sealed class IndexAnalysisRequestDto
{
    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string TableName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets a value indicating whether include fragmentation.
    /// </summary>
    public bool IncludeFragmentation { get; set; } = true;
    /// <summary>
    /// Gets or sets a value indicating whether include unused.
    /// </summary>
    public bool IncludeUnused { get; set; } = true;
    /// <summary>
    /// Gets or sets a value indicating whether generate scripts.
    /// </summary>
    public bool GenerateScripts { get; set; } = true;
}

/// <summary>
/// DTO for index analysis response
/// </summary>
public sealed class IndexAnalysisResponseDto
{
    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string TableName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the total indexes.
    /// </summary>
    public int TotalIndexes { get; set; }
    /// <summary>
    /// Gets or sets the unused count.
    /// </summary>
    public int UnusedCount { get; set; }
    /// <summary>
    /// Gets or sets the fragmented count.
    /// </summary>
    public int FragmentedCount { get; set; }
    /// <summary>
    /// Gets or sets the indexes.
    /// </summary>
    public List<IndexDetailDto> Indexes { get; set; } = [];
    /// <summary>
    /// Gets or sets the suggestions.
    /// </summary>
    public List<IndexSuggestionDto> Suggestions { get; set; } = [];
    /// <summary>
    /// Gets or sets the maintenance scripts.
    /// </summary>
    public List<string> MaintenanceScripts { get; set; } = [];
}

/// <summary>
/// DTO for index details
/// </summary>
public sealed class IndexDetailDto
{
    /// <summary>
    /// Gets or sets the index name.
    /// </summary>
    public string IndexName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the columns.
    /// </summary>
    public List<string> Columns { get; set; } = [];
    /// <summary>
    /// Gets or sets the size KB.
    /// </summary>
    public long SizeKB { get; set; }
    /// <summary>
    /// Gets or sets the fragmentation percent.
    /// </summary>
    public double FragmentationPercent { get; set; }
    /// <summary>
    /// Gets or sets the total usage count.
    /// </summary>
    public long TotalUsageCount { get; set; }
    /// <summary>
    /// Gets or sets the health status.
    /// </summary>
    public string HealthStatus { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the risk level.
    /// </summary>
    public string RiskLevel { get; set; } = string.Empty;
}
