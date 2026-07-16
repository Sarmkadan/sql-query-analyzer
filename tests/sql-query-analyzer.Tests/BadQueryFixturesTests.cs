#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using SqlQueryAnalyzer.Utilities;
using Xunit;

namespace SqlQueryAnalyzer.Tests;

/// <summary>
/// Correctness tests driven by the SQL files under <c>fixtures/</c>.
/// Each fixture is a known-bad query pattern; every test asserts that the
/// corresponding detection rule in <see cref="SqlPatternAnalyzer"/> actually fires,
/// and that the clean control fixture does not trip the same rules.
/// </summary>
public class BadQueryFixturesTests
{
    private static string FixturesDir =>
        Path.Combine(AppContext.BaseDirectory, "fixtures");

    /// <summary>
    /// Reads a fixture file and strips <c>--</c> comment lines and blank lines,
    /// returning the remaining SQL as a single normalized string.
    /// </summary>
    private static string LoadSql(string fileName)
    {
        var path = Path.Combine(FixturesDir, fileName);
        File.Exists(path).Should().BeTrue($"fixture '{fileName}' must be copied to the test output");

        var lines = File.ReadAllLines(path)
            .Where(l => !l.TrimStart().StartsWith("--"))
            .Where(l => !string.IsNullOrWhiteSpace(l));

        return string.Join(" ", lines).Trim();
    }

    /// <summary>
    /// Splits a multi-statement fixture into individual, comment-free statements.
    /// </summary>
    private static List<string> LoadStatements(string fileName) =>
        LoadSql(fileName)
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToList();

    [Fact]
    public void SelectStarFixture_TripsSelectStarRule()
    {
        var sql = LoadSql("select-star.sql");
        SqlPatternAnalyzer.HasSelectStar(sql).Should().BeTrue();
    }

    [Fact]
    public void MissingIndexFixture_TripsNonSargablePredicateRules()
    {
        var sql = LoadSql("missing-index.sql");

        // Function wrapping an indexed column defeats an index seek.
        SqlPatternAnalyzer.HasFunctionOnColumn(sql).Should().BeTrue();
        // Leading-wildcard LIKE cannot use a b-tree index range.
        SqlPatternAnalyzer.HasLeadingWildcardLike(sql).Should().BeTrue();
    }

    [Fact]
    public void CartesianJoinFixture_TripsImplicitJoinRule()
    {
        var sql = LoadSql("cartesian-join.sql");
        SqlPatternAnalyzer.HasImplicitJoin(sql).Should().BeTrue();
    }

    [Fact]
    public void NPlusOneFixture_TripsNPlusOneRule()
    {
        var statements = LoadStatements("n-plus-one.sql");
        statements.Count.Should().BeGreaterThan(5, "the fixture models a per-row loop");
        SqlPatternAnalyzer.DetectNPlusOnePattern(statements).Should().BeTrue();
    }

    [Fact]
    public void MissingWhereFixture_TripsMissingWhereRule()
    {
        var sql = LoadSql("missing-where.sql");
        SqlPatternAnalyzer.HasMissingWhereClause(sql).Should().BeTrue();
    }

    [Fact]
    public void EveryBadFixture_ProducesAtLeastOneRecommendation()
    {
        foreach (var fixture in new[]
                 {
                     "select-star.sql", "missing-index.sql",
                     "cartesian-join.sql", "missing-where.sql",
                 })
        {
            var sql = LoadSql(fixture);
            SqlPatternAnalyzer.GenerateOptimizationRecommendations(sql)
                .Should().NotBeEmpty($"'{fixture}' is a known-bad query");
        }
    }

    [Fact]
    public void CleanFixture_DoesNotTripBadQueryRules()
    {
        var sql = LoadSql("clean-query.sql");

        SqlPatternAnalyzer.HasSelectStar(sql).Should().BeFalse();
        SqlPatternAnalyzer.HasImplicitJoin(sql).Should().BeFalse();
        SqlPatternAnalyzer.HasLeadingWildcardLike(sql).Should().BeFalse();
        SqlPatternAnalyzer.HasFunctionOnColumn(sql).Should().BeFalse();
        SqlPatternAnalyzer.HasMissingWhereClause(sql).Should().BeFalse();
    }
}
