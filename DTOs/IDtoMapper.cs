#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using SqlQueryAnalyzer.Models;
using ModelIndex = SqlQueryAnalyzer.Models.Index;

namespace SqlQueryAnalyzer.DTOs;

/// <summary>
/// Interface for mapping domain models to DTOs.
/// Implementations are registered for DI and used by controllers.
/// </summary>
public interface IDtoMapper
{
    AnalysisResponseDto ToResponseDto(QueryAnalysisResult analysis, long analysisTimeMs);
    PerformanceIssueDto ToIssueDto(PerformanceIssue issue);
    IndexSuggestionDto ToSuggestionDto(IndexSuggestion suggestion);
    IndexDetailDto ToIndexDetailDto(ModelIndex index);
    IndexAnalysisResponseDto ToIndexAnalysisResponseDto(
        string tableName,
        List<ModelIndex> indexes,
        List<IndexSuggestion> suggestions,
        List<string> scripts);
    QueryDetailDto ToQueryDetailDto(DatabaseQuery query);
    BatchAnalysisResponseDto ToBatchResponseDto(
        List<QueryAnalysisResult> results,
        List<string> nPlusOnePatterns,
        long totalTimeMs);
}
