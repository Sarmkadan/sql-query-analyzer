#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace SqlQueryAnalyzer.Validation;

/// <summary>
/// Engine for validating SQL queries against configurable rules.
/// Supports regex patterns, syntax rules, and custom validators.
/// Provides detailed validation reports.
/// </summary>
public sealed class ValidationRuleEngine
{
    private readonly List<IValidationRule> _rules = new();
    private readonly ILogger<ValidationRuleEngine> _logger;

    public ValidationRuleEngine(ILogger<ValidationRuleEngine> logger)
    {
        _logger = logger;

        // Register default rules
        RegisterDefaultRules();
    }

    /// <summary>
    /// Validates a query against all rules.
    /// </summary>
    public ValidationResult ValidateQuery(string query)
    {
        var result = new ValidationResult();

        if (string.IsNullOrEmpty(query))
        {
            result.Errors.Add("Query cannot be empty");
            return result;
        }

        foreach (var rule in _rules)
        {
            try
            {
                var ruleResult = rule.Validate(query);
                if (!ruleResult.IsValid)
                {
                    result.Errors.AddRange(ruleResult.Errors);
                    result.Warnings.AddRange(ruleResult.Warnings);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing validation rule: {rule.Name}");
                result.Errors.Add($"Validation error: {ex.Message}");
            }
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    /// <summary>
    /// Registers a custom validation rule.
    /// </summary>
    public void RegisterRule(IValidationRule rule)
    {
        _rules.Add(rule);
        _logger.LogDebug("Registered validation rule: {Name}", rule.Name);
    }

    /// <summary>
    /// Registers default validation rules.
    /// </summary>
    private void RegisterDefaultRules()
    {
        RegisterRule(new SqlSyntaxRule());
        RegisterRule(new QueryLengthRule());
        RegisterRule(new DangerousOperationRule());
    }

    /// <summary>
    /// Gets count of registered rules.
    /// </summary>
    public int GetRuleCount() => _rules.Count;
}

/// <summary>
/// Base interface for validation rules.
/// </summary>
public interface IValidationRule
{
    /// <summary>
    /// Gets rule name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Validates query against this rule.
    /// </summary>
    RuleValidationResult Validate(string query);
}

/// <summary>
/// Validates basic SQL syntax.
/// </summary>
public sealed class SqlSyntaxRule : IValidationRule
{
    public string Name => "SQL Syntax";

    public RuleValidationResult Validate(string query)
    {
        var result = new RuleValidationResult();

        // Check for matching parentheses
        if (!HasMatchingParentheses(query))
        {
            result.Errors.Add("Mismatched parentheses");
        }

        // Check for matching quotes
        if (!HasMatchingQuotes(query))
        {
            result.Errors.Add("Mismatched quotes");
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    private bool HasMatchingParentheses(string query)
    {
        int count = 0;
        foreach (var c in query)
        {
            if (c == '(') count++;
            if (c == ')') count--;
            if (count < 0) return false;
        }

        return count == 0;
    }

    private bool HasMatchingQuotes(string query)
    {
        int singleQuotes = 0;
        int doubleQuotes = 0;

        for (int i = 0; i < query.Length; i++)
        {
            if (query[i] == '\'' && (i == 0 || query[i - 1] != '\\'))
                singleQuotes++;

            if (query[i] == '"' && (i == 0 || query[i - 1] != '\\'))
                doubleQuotes++;
        }

        return singleQuotes % 2 == 0 && doubleQuotes % 2 == 0;
    }
}

/// <summary>
/// Validates query length is within acceptable bounds.
/// </summary>
public class QueryLengthRule : IValidationRule
{
    public string Name => "Query Length";
    private const int MaxLength = 1024 * 1024; // 1 MB

    public RuleValidationResult Validate(string query)
    {
        var result = new RuleValidationResult();

        if (query.Length > MaxLength)
        {
            result.Errors.Add($"Query exceeds maximum length of {MaxLength} characters");
        }

        if (query.Length < 10)
        {
            result.Warnings.Add("Query is very short - may be incomplete");
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }
}

/// <summary>
/// Detects potentially dangerous operations.
/// </summary>
public class DangerousOperationRule : IValidationRule
{
    public string Name => "Dangerous Operations";

    public RuleValidationResult Validate(string query)
    {
        var result = new RuleValidationResult();
        var upperQuery = query.ToUpper();

        // Check for DELETE without WHERE
        if (upperQuery.Contains("DELETE") && !upperQuery.Contains("WHERE"))
        {
            result.Errors.Add("DELETE statement without WHERE clause detected");
        }

        // Check for DROP without confirmation
        if (upperQuery.Contains("DROP"))
        {
            result.Warnings.Add("DROP statement detected - use with caution");
        }

        // Check for multiple statements
        var statementCount = Regex.Matches(query, @";\s*\w+", RegexOptions.IgnoreCase).Count;
        if (statementCount > 1)
        {
            result.Warnings.Add($"Multiple SQL statements detected ({statementCount})");
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }
}

/// <summary>
/// Result of a single validation rule.
/// </summary>
public class RuleValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Overall validation result for a query.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public override string ToString() =>
        $"Valid: {IsValid}, Errors: {Errors.Count}, Warnings: {Warnings.Count}";
}
