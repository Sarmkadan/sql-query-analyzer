#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Validation helpers for StringExtensions methods
/// </summary>
public static class StringExtensionsValidation
{
    /// <summary>
    /// Validates the StringExtensions methods by checking all relevant members for common issues.
    /// </summary>
    /// <returns>A list of validation problems found, or an empty list if valid.</returns>
    public static IReadOnlyList<string> Validate()
    {
        var problems = new List<string>();

        // Validate NormalizeSqlWhitespace
        try
        {
            // Test with null input
            _ = ((string)null!).NormalizeSqlWhitespace();
            problems.Add("NormalizeSqlWhitespace does not throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch
        {
            problems.Add("NormalizeSqlWhitespace throws unexpected exception for null input");
        }

        // Validate RemoveSqlComments
        try
        {
            _ = ((string)null!).RemoveSqlComments();
            problems.Add("RemoveSqlComments does not throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch
        {
            problems.Add("RemoveSqlComments throws unexpected exception for null input");
        }

        // Validate Truncate
        try
        {
            _ = ((string)null!).Truncate(10);
            problems.Add("Truncate does not throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch
        {
            problems.Add("Truncate throws unexpected exception for null input");
        }

        try
        {
            _ = "test".Truncate(-1);
            problems.Add("Truncate does not throw ArgumentOutOfRangeException for negative maxLength");
        }
        catch (ArgumentOutOfRangeException) { /* Expected */ }
        catch
        {
            problems.Add("Truncate throws unexpected exception for negative maxLength");
        }

        // Validate IsSqlKeyword
        try
        {
            _ = ((string)null!).IsSqlKeyword();
            problems.Add("IsSqlKeyword does not throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch
        {
            problems.Add("IsSqlKeyword throws unexpected exception for null input");
        }

        // Validate CapitalizeFirst
        try
        {
            _ = ((string)null!).CapitalizeFirst();
            problems.Add("CapitalizeFirst does not throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch
        {
            problems.Add("CapitalizeFirst throws unexpected exception for null input");
        }

        // Validate ToSnakeCase
        try
        {
            _ = ((string)null!).ToSnakeCase();
            problems.Add("ToSnakeCase does not throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch
        {
            problems.Add("ToSnakeCase throws unexpected exception for null input");
        }

        // Validate CountOccurrences
        try
        {
            _ = ((string)null!).CountOccurrences("test");
            problems.Add("CountOccurrences does not throw ArgumentNullException for null text input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch
        {
            problems.Add("CountOccurrences throws unexpected exception for null text input");
        }

        try
        {
            _ = "test".CountOccurrences(null!);
            problems.Add("CountOccurrences does not throw ArgumentNullException for null substring input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch
        {
            problems.Add("CountOccurrences throws unexpected exception for null substring input");
        }

        // Validate ContainsSuspiciousPatterns
        try
        {
            _ = ((string)null!).ContainsSuspiciousPatterns();
            problems.Add("ContainsSuspiciousPatterns does not throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch
        {
            problems.Add("ContainsSuspiciousPatterns throws unexpected exception for null input");
        }

        // Validate ExtractQueryType
        try
        {
            _ = ((string)null!).ExtractQueryType();
            problems.Add("ExtractQueryType does not throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch
        {
            problems.Add("ExtractQueryType throws unexpected exception for null input");
        }

        // Validate SplitStatements
        try
        {
            _ = ((string)null!).SplitStatements();
            problems.Add("SplitStatements does not throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch
        {
            problems.Add("SplitStatements throws unexpected exception for null input");
        }

        // Validate GetPosition
        try
        {
            _ = ((string)null!).GetPosition(0);
            problems.Add("GetPosition does not throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected */ }
        catch
        {
            problems.Add("GetPosition throws unexpected exception for null input");
        }

        try
        {
            _ = "test".GetPosition(-1);
            problems.Add("GetPosition does not throw ArgumentOutOfRangeException for negative index");
        }
        catch (ArgumentOutOfRangeException) { /* Expected */ }
        catch
        {
            problems.Add("GetPosition throws unexpected exception for negative index");
        }

        try
        {
            _ = "test".GetPosition(10);
            problems.Add("GetPosition does not throw ArgumentOutOfRangeException for index greater than length");
        }
        catch (ArgumentOutOfRangeException) { /* Expected */ }
        catch
        {
            problems.Add("GetPosition throws unexpected exception for index greater than length");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the StringExtensions methods are valid.
    /// </summary>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid() => Validate().Count == 0;

    /// <summary>
    /// Ensures that the StringExtensions methods are valid, throwing an exception if not.
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