#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.DTOs;

/// <summary>
/// Maps between domain models and DTOs
/// </summary>
public static class DtoMapper
{
    // Map QueryAnalysisResult to AnalysisResponseDto
    public static AnalysisResponseDto ToResponseDto(QueryAnalysisResult analysis, long analysisTimeMs)
    {
        return new AnalysisResponseDto
        {
            QueryId = analysis.QueryId,
            PerformanceScore = analysis.PerformanceScore,
            ComplexityLevel = analysis.Complexity.ToString(),
            IssueCount = analysis.Issues.Count,
            CriticalIssueCount = analysis.Issues.Count(i => i.Severity == Constants.IssueSeverity.Critical),
            Issues = analysis.Issues.Select(ToIssueDto).ToList(),
            IndexSuggestions = analysis.IndexSuggestions.Select(ToSuggestionDto).ToList(),
            Summary = GetSummary(analysis),
            AnalysisTimeMs = analysisTimeMs
        };
    }

    // Map PerformanceIssue to PerformanceIssueDto
    public static PerformanceIssueDto ToIssueDto(PerformanceIssue issue)
    {
        return new PerformanceIssueDto
        {
            IssueType = issue.IssueType.ToString(),
            Severity = issue.Severity.ToString(),
            Description = issue.Description,
            EstimatedImpact = issue.EstimatedPerformanceImpact,
            RecommendedFix = issue.RecommendedFix,
            Priority = issue.Priority
        };
    }

    // Map IndexSuggestion to IndexSuggestionDto
    public static IndexSuggestionDto ToSuggestionDto(IndexSuggestion suggestion)
    {
        return new IndexSuggestionDto
        {
            TableName = suggestion.TableName,
            IndexName = suggestion.IndexName,
            Columns = suggestion.IndexColumns,
            IncludeColumns = suggestion.IncludeColumns,
            EstimatedGain = suggestion.EstimatedPerformanceGain,
            CreateScript = suggestion.GeneratedCreateScript,
            AffectedQueries = suggestion.AffectedQueries
        };
    }

    // Map Index to IndexDetailDto
    public static IndexDetailDto ToIndexDetailDto(Index index)
    {
        return new IndexDetailDto
        {
            IndexName = index.IndexName,
            Type = index.IndexType.ToString(),
            Columns = index.Columns.OrderBy(c => c.KeyOrdinal).Select(c => c.ColumnName).ToList(),
            SizeKB = index.SizeInBytes / 1024,
            FragmentationPercent = index.FragmentationPercentage,
            TotalUsageCount = index.TotalUsageCount,
            HealthStatus = index.HealthStatus.ToString(),
            RiskLevel = index.EstimateCost().ToString()
        };
    }

    // Map Index list to IndexAnalysisResponseDto
    public static IndexAnalysisResponseDto ToIndexAnalysisResponseDto(
        string tableName,
        List<Index> indexes,
        List<IndexSuggestion> suggestions,
        List<string> scripts)
    {
        return new IndexAnalysisResponseDto
        {
            TableName = tableName,
            TotalIndexes = indexes.Count,
            UnusedCount = indexes.Count(i => i.IsCandidateForRemoval),
            FragmentedCount = indexes.Count(i => i.IsFragmented),
            Indexes = indexes.Select(ToIndexDetailDto).ToList(),
            Suggestions = suggestions.Select(ToSuggestionDto).ToList(),
            MaintenanceScripts = scripts
        };
    }

    // Map DatabaseQuery for display
    public static QueryDetailDto ToQueryDetailDto(DatabaseQuery query)
    {
        return new QueryDetailDto
        {
            QueryId = query.QueryId,
            QueryText = query.QueryText,
            QueryType = query.QueryType.ToString(),
            TableCount = query.ReferencedTables.Count,
            Tables = query.ReferencedTables,
            JoinCount = query.JoinConditions.Count,
            ParameterCount = query.Parameters.Count,
            LineCount = query.LineCount
        };
    }

    // Map batch analysis results
    public static BatchAnalysisResponseDto ToBatchResponseDto(
        List<QueryAnalysisResult> results,
        List<string> nPlusOnePatterns,
        long totalTimeMs)
    {
        var responses = results.Select(r => ToResponseDto(r, 0)).ToList();

        return new BatchAnalysisResponseDto
        {
            TotalQueries = results.Count,
            SuccessfulAnalyses = results.Count,
            FailedAnalyses = 0,
            Results = responses,
            NPlusOnePatterns = nPlusOnePatterns,
            AverageScore = results.Count > 0 ? results.Average(r => r.PerformanceScore) : 0,
            TotalAnalysisTimeMs = totalTimeMs
        };
    }

    // Create summary text
    private static string GetSummary(QueryAnalysisResult analysis)
    {
        var criticalCount = analysis.Issues.Count(i => i.Severity == Constants.IssueSeverity.Critical);
        var warningCount = analysis.Issues.Count(i => i.Severity == Constants.IssueSeverity.Warning);
        var infoCount = analysis.Issues.Count(i => i.Severity == Constants.IssueSeverity.Info);

        return $"Score: {analysis.PerformanceScore:F0}/100 | " +
               $"Issues: {analysis.Issues.Count} ({criticalCount} critical, {warningCount} warnings, {infoCount} info) | " +
               $"Suggestions: {analysis.IndexSuggestions.Count} | " +
               $"Optimization Potential: {analysis.TotalOptimizationPotential:F1}%";
    }
}

/// <summary>
/// DTO for query details
/// </summary>
public sealed class QueryDetailDto
{
    public string QueryId { get; set; } = string.Empty;
    public string QueryText { get; set; } = string.Empty;
    public string QueryType { get; set; } = string.Empty;
    public int TableCount { get; set; }
    public List<string> Tables { get; set; } = [];
    public int JoinCount { get; set; }
    public int ParameterCount { get; set; }
    public int LineCount { get; set; }
}
