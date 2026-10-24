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
    /// <summary>
    /// To response DTO.
    /// </summary>
    AnalysisResponseDto ToResponseDto(QueryAnalysisResult analysis, long analysisTimeMs);
    /// <summary>
    /// To issue DTO.
    /// </summary>
    PerformanceIssueDto ToIssueDto(PerformanceIssue issue);
    /// <summary>
    /// To suggestion DTO.
    /// </summary>
    IndexSuggestionDto ToSuggestionDto(IndexSuggestion suggestion);
    /// <summary>
    /// To index detail DTO.
    /// </summary>
    IndexDetailDto ToIndexDetailDto(ModelIndex index);
    /// <summary>
    /// To index analysis response DTO.
    /// </summary>
    IndexAnalysisResponseDto ToIndexAnalysisResponseDto(
        string tableName,
        List<ModelIndex> indexes,
        List<IndexSuggestion> suggestions,
        List<string> scripts);
    /// <summary>
    /// To query detail DTO.
    /// </summary>
    QueryDetailDto ToQueryDetailDto(DatabaseQuery query);
    /// <summary>
    /// To batch response DTO.
    /// </summary>
    BatchAnalysisResponseDto ToBatchResponseDto(
        List<QueryAnalysisResult> results,
        List<string> nPlusOnePatterns,
        long totalTimeMs);
}
