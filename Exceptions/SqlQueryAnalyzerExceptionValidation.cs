#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
// ReSharper disable LocalizableElement

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SqlQueryAnalyzer.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="SqlQueryAnalyzerException"/> and its derived types.
/// </summary>
/// <remarks>
/// This class cannot be inherited.
/// </remarks>
public static class SqlQueryAnalyzerExceptionValidation
{
    /// <summary>
    /// Validates an exception instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>An enumerable of validation problems; empty if the exception is valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SqlQueryAnalyzerException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        switch (value)
        {
            case InvalidQueryException invalidQueryException:
                ValidateInvalidQueryException(invalidQueryException, problems);
                break;

            case DatabaseConnectionException dbConnectionException:
                ValidateDatabaseConnectionException(dbConnectionException, problems);
                break;

            case QueryPlanException queryPlanException:
                ValidateQueryPlanException(queryPlanException, problems);
                break;

            case IndexAnalysisException indexAnalysisException:
                ValidateIndexAnalysisException(indexAnalysisException, problems);
                break;

            case ConfigurationException configurationException:
                ValidateConfigurationException(configurationException, problems);
                break;

            case RepositoryException repositoryException:
                ValidateRepositoryException(repositoryException, problems);
                break;

            case ValidationException validationException:
                ValidateValidationException(validationException, problems);
                break;

            case IntegrationException integrationException:
                ValidateIntegrationException(integrationException, problems);
                break;

            case AnalysisException analysisException:
                ValidateAnalysisException(analysisException, problems);
                break;
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an exception instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns>True if the exception is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static bool IsValid(this SqlQueryAnalyzerException value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that an exception instance is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message if it is not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is invalid.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static void EnsureValid(this SqlQueryAnalyzerException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"The exception is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates the base <see cref="AnalysisException"/> properties.
    /// </summary>
    /// <param name="exception">The exception to validate.</param>
    /// <param name="problems">The list to accumulate validation problems.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> or <paramref name="problems"/> is null.</exception>
    private static void ValidateAnalysisException(AnalysisException exception, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(problems);

        if (string.IsNullOrWhiteSpace(exception.ErrorCode))
        {
            problems.Add("AnalysisException.ErrorCode must not be null, empty, or whitespace.");
        }

        if (exception.ErrorDetails is not null && string.IsNullOrWhiteSpace(exception.ErrorDetails))
        {
            problems.Add("AnalysisException.ErrorDetails must not be empty or whitespace if set.");
        }
    }

    /// <summary>
    /// Validates the <see cref="InvalidQueryException"/> properties.
    /// </summary>
    /// <param name="exception">The exception to validate.</param>
    /// <param name="problems">The list to accumulate validation problems.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> or <paramref name="problems"/> is null.</exception>
    private static void ValidateInvalidQueryException(InvalidQueryException exception, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(problems);

        ValidateAnalysisException(exception, problems);

        if (string.IsNullOrWhiteSpace(exception.Query))
        {
            problems.Add("InvalidQueryException.Query must not be null, empty, or whitespace.");
        }

        if (exception.LineNumber.HasValue && exception.LineNumber.Value < 0)
        {
            problems.Add("InvalidQueryException.LineNumber must be a non-negative integer if set.");
        }

        if (exception.ColumnNumber.HasValue && exception.ColumnNumber.Value < 0)
        {
            problems.Add("InvalidQueryException.ColumnNumber must be a non-negative integer if set.");
        }
    }

    /// <summary>
    /// Validates the <see cref="DatabaseConnectionException"/> properties.
    /// </summary>
    /// <param name="exception">The exception to validate.</param>
    /// <param name="problems">The list to accumulate validation problems.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> or <paramref name="problems"/> is null.</exception>
    private static void ValidateDatabaseConnectionException(DatabaseConnectionException exception, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(problems);

        ValidateAnalysisException(exception, problems);

        if (string.IsNullOrWhiteSpace(exception.ConnectionString))
        {
            problems.Add("DatabaseConnectionException.ConnectionString must not be null, empty, or whitespace.");
        }

        if (exception.DatabaseName is not null && string.IsNullOrWhiteSpace(exception.DatabaseName))
        {
            problems.Add("DatabaseConnectionException.DatabaseName must not be empty or whitespace if set.");
        }
    }

    /// <summary>
    /// Validates the <see cref="QueryPlanException"/> properties.
    /// </summary>
    /// <param name="exception">The exception to validate.</param>
    /// <param name="problems">The list to accumulate validation problems.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> or <paramref name="problems"/> is null.</exception>
    private static void ValidateQueryPlanException(QueryPlanException exception, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(problems);

        ValidateAnalysisException(exception, problems);

        if (string.IsNullOrWhiteSpace(exception.PlanSource))
        {
            problems.Add("QueryPlanException.PlanSource must not be null, empty, or whitespace.");
        }
    }

    /// <summary>
    /// Validates the <see cref="IndexAnalysisException"/> properties.
    /// </summary>
    /// <param name="exception">The exception to validate.</param>
    /// <param name="problems">The list to accumulate validation problems.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> or <paramref name="problems"/> is null.</exception>
    private static void ValidateIndexAnalysisException(IndexAnalysisException exception, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(problems);

        ValidateAnalysisException(exception, problems);

        if (exception.IndexName is not null && string.IsNullOrWhiteSpace(exception.IndexName))
        {
            problems.Add("IndexAnalysisException.IndexName must not be empty or whitespace if set.");
        }

        if (exception.TableName is not null && string.IsNullOrWhiteSpace(exception.TableName))
        {
            problems.Add("IndexAnalysisException.TableName must not be empty or whitespace if set.");
        }
    }

    /// <summary>
    /// Validates the <see cref="ConfigurationException"/> properties.
    /// </summary>
    /// <param name="exception">The exception to validate.</param>
    /// <param name="problems">The list to accumulate validation problems.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> or <paramref name="problems"/> is null.</exception>
    private static void ValidateConfigurationException(ConfigurationException exception, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(problems);

        ValidateAnalysisException(exception, problems);

        if (exception.ConfigKey is not null && string.IsNullOrWhiteSpace(exception.ConfigKey))
        {
            problems.Add("ConfigurationException.ConfigKey must not be empty or whitespace if set.");
        }
    }

    /// <summary>
    /// Validates the <see cref="RepositoryException"/> properties.
    /// </summary>
    /// <param name="exception">The exception to validate.</param>
    /// <param name="problems">The list to accumulate validation problems.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> or <paramref name="problems"/> is null.</exception>
    private static void ValidateRepositoryException(RepositoryException exception, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(problems);

        ValidateAnalysisException(exception, problems);

        if (exception.OperationType is not null && string.IsNullOrWhiteSpace(exception.OperationType))
        {
            problems.Add("RepositoryException.OperationType must not be empty or whitespace if set.");
        }

        if (exception.ResourceId is not null && string.IsNullOrWhiteSpace(exception.ResourceId))
        {
            problems.Add("RepositoryException.ResourceId must not be empty or whitespace if set.");
        }
    }

    /// <summary>
    /// Validates the <see cref="ValidationException"/> properties.
    /// </summary>
    /// <param name="exception">The exception to validate.</param>
    /// <param name="problems">The list to accumulate validation problems.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> or <paramref name="problems"/> is null.</exception>
    private static void ValidateValidationException(ValidationException exception, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(problems);

        ValidateAnalysisException(exception, problems);

        if (exception.ValidationField is not null && string.IsNullOrWhiteSpace(exception.ValidationField))
        {
            problems.Add("ValidationException.ValidationField must not be empty or whitespace if set.");
        }

        if (exception.ValidationRule is not null && string.IsNullOrWhiteSpace(exception.ValidationRule))
        {
            problems.Add("ValidationException.ValidationRule must not be empty or whitespace if set.");
        }
    }

    /// <summary>
    /// Validates the <see cref="IntegrationException"/> properties.
    /// </summary>
    /// <param name="exception">The exception to validate.</param>
    /// <param name="problems">The list to accumulate validation problems.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> or <paramref name="problems"/> is null.</exception>
    private static void ValidateIntegrationException(IntegrationException exception, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(problems);

        ValidateAnalysisException(exception, problems);

        if (exception.ServiceName is not null && string.IsNullOrWhiteSpace(exception.ServiceName))
        {
            problems.Add("IntegrationException.ServiceName must not be empty or whitespace if set.");
        }
    }
}