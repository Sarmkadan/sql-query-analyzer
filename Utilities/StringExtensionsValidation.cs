#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides validation for <see cref="StringExtensions"/> extension methods to ensure they properly handle null inputs and throw appropriate exceptions.
/// </summary>
public static class StringExtensionsValidation
{
    /// <summary>
    /// Validates that all <see cref="StringExtensions"/> methods properly handle null inputs and throw appropriate exceptions.
    /// </summary>
    /// <returns>A list of validation problems found, or an empty list if all methods are valid.</returns>
    /// <exception cref="InvalidOperationException">Thrown if validation encounters an unexpected error.</exception>
    public static IReadOnlyList<string> Validate()
    {
        var problems = new List<string>();

        // Validate NormalizeSqlWhitespace
        try
        {
            _ = string.Empty.NormalizeSqlWhitespace(); // Test with valid input first
        }
        catch (Exception ex)
        {
            problems.Add($"NormalizeSqlWhitespace throws unexpected exception for valid input: {ex.Message}");
        }

        try
        {
            _ = ((string)null!).NormalizeSqlWhitespace();
            problems.Add("NormalizeSqlWhitespace does not throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch (Exception ex)
        {
            problems.Add($"NormalizeSqlWhitespace throws unexpected exception for null input: {ex.GetType().Name}");
        }

        // Validate RemoveSqlComments
        try
        {
            _ = string.Empty.RemoveSqlComments(); // Test with valid input first
        }
        catch (Exception ex)
        {
            problems.Add($"RemoveSqlComments throws unexpected exception for valid input: {ex.Message}");
        }

        try
        {
            _ = ((string)null!).RemoveSqlComments();
            problems.Add("RemoveSqlComments does not throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch (Exception ex)
        {
            problems.Add($"RemoveSqlComments throws unexpected exception for null input: {ex.GetType().Name}");
        }

        // Validate Truncate
        try
        {
            _ = string.Empty.Truncate(10); // Test with valid input first
        }
        catch (Exception ex)
        {
            problems.Add($"Truncate throws unexpected exception for valid input: {ex.Message}");
        }

        try
        {
            _ = ((string)null!).Truncate(10);
            problems.Add("Truncate does not throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch (Exception ex)
        {
            problems.Add($"Truncate throws unexpected exception for null input: {ex.GetType().Name}");
        }

        try
        {
            _ = "test".Truncate(-1);
            problems.Add("Truncate does not throw ArgumentOutOfRangeException for negative maxLength");
        }
        catch (ArgumentOutOfRangeException) { /* Expected */ }
        catch (Exception ex)
        {
            problems.Add($"Truncate throws unexpected exception for negative maxLength: {ex.GetType().Name}");
        }

        // Validate IsSqlKeyword
        try
        {
            _ = string.Empty.IsSqlKeyword(); // Test with valid input first
        }
        catch (Exception ex)
        {
            problems.Add($"IsSqlKeyword throws unexpected exception for valid input: {ex.Message}");
        }

        try
        {
            _ = ((string)null!).IsSqlKeyword();
            problems.Add("IsSqlKeyword does not throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch (Exception ex)
        {
            problems.Add($"IsSqlKeyword throws unexpected exception for null input: {ex.GetType().Name}");
        }

        // Validate CapitalizeFirst
        try
        {
            _ = string.Empty.CapitalizeFirst(); // Test with valid input first
        }
        catch (Exception ex)
        {
            problems.Add($"CapitalizeFirst throws unexpected exception for valid input: {ex.Message}");
        }

        try
        {
            _ = ((string)null!).CapitalizeFirst();
            problems.Add("CapitalizeFirst does not throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch (Exception ex)
        {
            problems.Add($"CapitalizeFirst throws unexpected exception for null input: {ex.GetType().Name}");
        }

        // Validate ToSnakeCase
        try
        {
            _ = string.Empty.ToSnakeCase(); // Test with valid input first
        }
        catch (Exception ex)
        {
            problems.Add($"ToSnakeCase throws unexpected exception for valid input: {ex.Message}");
        }

        try
        {
            _ = ((string)null!).ToSnakeCase();
            problems.Add("ToSnakeCase does not throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch (Exception ex)
        {
            problems.Add($"ToSnakeCase throws unexpected exception for null input: {ex.GetType().Name}");
        }

        // Validate CountOccurrences
        try
        {
            _ = string.Empty.CountOccurrences("test"); // Test with valid input first
        }
        catch (Exception ex)
        {
            problems.Add($"CountOccurrences throws unexpected exception for valid input: {ex.Message}");
        }

        try
        {
            _ = ((string)null!).CountOccurrences("test");
            problems.Add("CountOccurrences does not throw ArgumentNullException for null text input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch (Exception ex)
        {
            problems.Add($"CountOccurrences throws unexpected exception for null text input: {ex.GetType().Name}");
        }

        try
        {
            _ = "test".CountOccurrences(null!);
            problems.Add("CountOccurrences does not throw ArgumentNullException for null substring input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch (Exception ex)
        {
            problems.Add($"CountOccurrences throws unexpected exception for null substring input: {ex.GetType().Name}");
        }

        // Validate ContainsSuspiciousPatterns
        try
        {
            _ = string.Empty.ContainsSuspiciousPatterns(); // Test with valid input first
        }
        catch (Exception ex)
        {
            problems.Add($"ContainsSuspiciousPatterns throws unexpected exception for valid input: {ex.Message}");
        }

        try
        {
            _ = ((string)null!).ContainsSuspiciousPatterns();
            problems.Add("ContainsSuspiciousPatterns does not throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch (Exception ex)
        {
            problems.Add($"ContainsSuspiciousPatterns throws unexpected exception for null input: {ex.GetType().Name}");
        }

        // Validate ExtractQueryType
        try
        {
            _ = string.Empty.ExtractQueryType(); // Test with valid input first
        }
        catch (Exception ex)
        {
            problems.Add($"ExtractQueryType throws unexpected exception for valid input: {ex.Message}");
        }

        try
        {
            _ = ((string)null!).ExtractQueryType();
            problems.Add("ExtractQueryType does not throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch (Exception ex)
        {
            problems.Add($"ExtractQueryType throws unexpected exception for null input: {ex.GetType().Name}");
        }

        // Validate SplitStatements
        try
        {
            _ = string.Empty.SplitStatements(); // Test with valid input first
        }
        catch (Exception ex)
        {
            problems.Add($"SplitStatements throws unexpected exception for valid input: {ex.Message}");
        }

        try
        {
            _ = ((string)null!).SplitStatements();
            problems.Add("SplitStatements does not throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch (Exception ex)
        {
            problems.Add($"SplitStatements throws unexpected exception for null input: {ex.GetType().Name}");
        }

        // Validate GetPosition
        try
        {
            _ = string.Empty.GetPosition(0); // Test with valid input first
        }
        catch (Exception ex)
        {
            problems.Add($"GetPosition throws unexpected exception for valid input: {ex.Message}");
        }

        try
        {
            _ = ((string)null!).GetPosition(0);
            problems.Add("GetPosition does not throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch (Exception ex)
        {
            problems.Add($"GetPosition throws unexpected exception for null input: {ex.GetType().Name}");
        }

        try
        {
            _ = "test".GetPosition(-1);
            problems.Add("GetPosition does not throw ArgumentOutOfRangeException for negative index");
        }
        catch (ArgumentOutOfRangeException) { /* Expected */ }
        catch (Exception ex)
        {
            problems.Add($"GetPosition throws unexpected exception for negative index: {ex.GetType().Name}");
        }

        try
        {
            _ = "test".GetPosition(10);
            problems.Add("GetPosition does not throw ArgumentOutOfRangeException for index greater than length");
        }
        catch (ArgumentOutOfRangeException) { /* Expected */ }
        catch (Exception ex)
        {
            problems.Add($"GetPosition throws unexpected exception for index greater than length: {ex.GetType().Name}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="StringExtensions"/> methods are valid.
    /// </summary>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid() => Validate().Count == 0;

    /// <summary>
    /// Ensures that the <see cref="StringExtensions"/> methods are valid, throwing an exception if not.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if validation fails.</exception>
    public static void EnsureValid()
    {
        var problems = Validate();
        if (problems.Count == 0)
            return;

        throw new ArgumentException(
            $"StringExtensions validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }
}