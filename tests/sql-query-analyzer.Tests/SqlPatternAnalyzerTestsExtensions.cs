using System;
using SqlQueryAnalyzer.Tests;

namespace SqlQueryAnalyzer.Tests;

/// <summary>
/// Provides extension methods for the <see cref="SqlPatternAnalyzerTests"/> class to facilitate execution of related test groups.
/// </summary>
public static class SqlPatternAnalyzerTestsExtensions
{
    /// <summary>
    /// Executes all tests related to SELECT * pattern detection and optimization recommendations.
    /// </summary>
    /// <param name="tests">The <see cref="SqlPatternAnalyzerTests"/> instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    public static void ExecuteAllSelectStarTests(this SqlPatternAnalyzerTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        tests.HasSelectStar_QueryContainsStar_ReturnsTrue();
        tests.HasSelectStar_QueryWithNamedColumns_ReturnsFalse();
        tests.GenerateOptimizationRecommendations_SelectStarQuery_IncludesColumnReplacementAdvice();
    }

    /// <summary>
    /// Executes all tests related to N+1 pattern detection.
    /// </summary>
    /// <param name="tests">The <see cref="SqlPatternAnalyzerTests"/> instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    public static void ExecuteAllNPlusOneTests(this SqlPatternAnalyzerTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        tests.DetectNPlusOnePattern_SingleQueryInList_ReturnsFalse();
        tests.DetectNPlusOnePattern_SameTableAccessedMoreThanFiveTimes_ReturnsTrue();
    }

    /// <summary>
    /// Executes all tests related to readability score calculation.
    /// </summary>
    /// <param name="tests">The <see cref="SqlPatternAnalyzerTests"/> instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    public static void ExecuteAllReadabilityTests(this SqlPatternAnalyzerTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        tests.CalculateReadabilityScore_WellWrittenQuery_ReturnsFullScore();
        tests.CalculateReadabilityScore_SelectStarWithImplicitJoin_DeductsThirtyPoints();
    }

    /// <summary>
    /// Executes all remaining pattern detection tests.
    /// </summary>
    /// <param name="tests">The <see cref="SqlPatternAnalyzerTests"/> instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    public static void ExecuteAllPatternDetectionTests(this SqlPatternAnalyzerTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        tests.HasLeadingWildcardLike_PatternStartsWithPercent_ReturnsTrue();
    }
}
