#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using SqlQueryAnalyzer.Models;
using ModelIndex = SqlQueryAnalyzer.Models.Index;
using SqlQueryAnalyzer.Constants;

namespace SqlQueryAnalyzer.DTOs;

/// <summary>
/// Maps between domain models and DTOs.
/// This class implements <see cref="IDtoMapper"/> and is registered for DI.
/// </summary>
public class DtoMapper : IDtoMapper
{
    // Map QueryAnalysisResult to AnalysisResponseDto
    /// <summary>
    /// To response DTO.
    /// </summary>
    public AnalysisResponseDto ToResponseDto(QueryAnalysisResult analysis, long analysisTimeMs)
    {
        ArgumentNullException.ThrowIfNull(analysis);
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
    /// <summary>
    /// To issue DTO.
    /// </summary>
    public PerformanceIssueDto ToIssueDto(PerformanceIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);
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
    /// <summary>
    /// To suggestion DTO.
    /// </summary>
    public IndexSuggestionDto ToSuggestionDto(IndexSuggestion suggestion)
    {
        ArgumentNullException.ThrowIfNull(suggestion);
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
    /// <summary>
    /// To index detail DTO.
    /// </summary>
    public IndexDetailDto ToIndexDetailDto(ModelIndex index)
    {
        ArgumentNullException.ThrowIfNull(index);
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
    /// <summary>
    /// To index analysis response DTO.
    /// </summary>
    public IndexAnalysisResponseDto ToIndexAnalysisResponseDto(
        string tableName,
        List<ModelIndex> indexes,
        List<IndexSuggestion> suggestions,
        List<string> scripts)
    {
        ArgumentException.ThrowIfNullOrEmpty(tableName);
        ArgumentNullException.ThrowIfNull(indexes);
        ArgumentNullException.ThrowIfNull(suggestions);
        ArgumentNullException.ThrowIfNull(scripts);
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
    /// <summary>
    /// To query detail DTO.
    /// </summary>
    public QueryDetailDto ToQueryDetailDto(DatabaseQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
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
    /// <summary>
    /// To batch response DTO.
    /// </summary>
    public BatchAnalysisResponseDto ToBatchResponseDto(
        List<QueryAnalysisResult> results,
        List<string> nPlusOnePatterns,
        long totalTimeMs)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentNullException.ThrowIfNull(nPlusOnePatterns);
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
    private string GetSummary(QueryAnalysisResult analysis)
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
    /// <summary>
    /// Gets or sets the query ID.
    /// </summary>
    public string QueryId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the query text.
    /// </summary>
    public string QueryText { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the query type.
    /// </summary>
    public string QueryType { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the table count.
    /// </summary>
    public int TableCount { get; set; }
    /// <summary>
    /// Gets or sets the tables.
    /// </summary>
    public List<string> Tables { get; set; } = [];
    /// <summary>
    /// Gets or sets the join count.
    /// </summary>
    public int JoinCount { get; set; }
    /// <summary>
    /// Gets or sets the parameter count.
    /// </summary>
    public int ParameterCount { get; set; }
    /// <summary>
    /// Gets or sets the line count.
    /// </summary>
    public int LineCount { get; set; }
}
