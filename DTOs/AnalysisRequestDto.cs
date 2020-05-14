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
    public string QueryText { get; set; } = string.Empty;
    public string? ApplicationName { get; set; }
    public string? ProcedureName { get; set; }
    public string? ModuleName { get; set; }
    public bool IncludeIndexSuggestions { get; set; } = true;
    public bool AnalyzeFragmentation { get; set; } = true;
    public bool AnalyzePlan { get; set; } = false;
    public string? ExecutionPlanXml { get; set; }
}

/// <summary>
/// Response DTO for analysis results
/// </summary>
public sealed class AnalysisResponseDto
{
    public string QueryId { get; set; } = string.Empty;
    public double PerformanceScore { get; set; }
    public string ComplexityLevel { get; set; } = string.Empty;
    public int IssueCount { get; set; }
    public int CriticalIssueCount { get; set; }
    public List<PerformanceIssueDto> Issues { get; set; } = [];
    public List<IndexSuggestionDto> IndexSuggestions { get; set; } = [];
    public string Summary { get; set; } = string.Empty;
    public long AnalysisTimeMs { get; set; }
}

/// <summary>
/// DTO for performance issue
/// </summary>
public sealed class PerformanceIssueDto
{
    public string IssueType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double EstimatedImpact { get; set; }
    public string RecommendedFix { get; set; } = string.Empty;
    public int Priority { get; set; }
}

/// <summary>
/// DTO for index suggestion
/// </summary>
public sealed class IndexSuggestionDto
{
    public string TableName { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = [];
    public List<string> IncludeColumns { get; set; } = [];
    public double EstimatedGain { get; set; }
    public string CreateScript { get; set; } = string.Empty;
    public int AffectedQueries { get; set; }
}

/// <summary>
/// DTO for batch analysis request
/// </summary>
public sealed class BatchAnalysisRequestDto
{
    public List<string> Queries { get; set; } = [];
    public string? ApplicationName { get; set; }
    public bool AnalyzePatterns { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 300;
}

/// <summary>
/// DTO for batch analysis response
/// </summary>
public sealed class BatchAnalysisResponseDto
{
    public int TotalQueries { get; set; }
    public int SuccessfulAnalyses { get; set; }
    public int FailedAnalyses { get; set; }
    public List<AnalysisResponseDto> Results { get; set; } = [];
    public List<string> NPlusOnePatterns { get; set; } = [];
    public double AverageScore { get; set; }
    public long TotalAnalysisTimeMs { get; set; }
}

/// <summary>
/// DTO for index analysis request
/// </summary>
public sealed class IndexAnalysisRequestDto
{
    public string TableName { get; set; } = string.Empty;
    public bool IncludeFragmentation { get; set; } = true;
    public bool IncludeUnused { get; set; } = true;
    public bool GenerateScripts { get; set; } = true;
}

/// <summary>
/// DTO for index analysis response
/// </summary>
public sealed class IndexAnalysisResponseDto
{
    public string TableName { get; set; } = string.Empty;
    public int TotalIndexes { get; set; }
    public int UnusedCount { get; set; }
    public int FragmentedCount { get; set; }
    public List<IndexDetailDto> Indexes { get; set; } = [];
    public List<IndexSuggestionDto> Suggestions { get; set; } = [];
    public List<string> MaintenanceScripts { get; set; } = [];
}

/// <summary>
/// DTO for index details
/// </summary>
public sealed class IndexDetailDto
{
    public string IndexName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = [];
    public long SizeKB { get; set; }
    public double FragmentationPercent { get; set; }
    public long TotalUsageCount { get; set; }
    public string HealthStatus { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
}
