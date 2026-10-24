#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SqlQueryAnalyzer.Exceptions;

/// <summary>
/// Base exception for all SQL Query Analyzer errors.
/// </summary>
public class SqlQueryAnalyzerException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlQueryAnalyzerException"/> class.
    /// </summary>
    public SqlQueryAnalyzerException(string message) : base(message) { }
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlQueryAnalyzerException"/> class.
    /// </summary>
    public SqlQueryAnalyzerException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    public override string ToString()
    {
        var propertyNames = new[] { "ErrorCode", "ErrorDetails", "Query", "LineNumber", "ColumnNumber", "ConnectionString" };
        var props = new List<string>();
        foreach (var name in propertyNames)
        {
            var prop = this.GetType().GetProperty(name);
            if (prop != null)
            {
                var value = prop.GetValue(this);
                props.Add($"{name} = {value}");
            }
        }

        return $"SqlQueryAnalyzerException {{ {string.Join(", ", props)} }}";
    }
}

/// <summary>
/// Base exception for all analysis-related errors
/// </summary>
public class AnalysisException : SqlQueryAnalyzerException
{
    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string? ErrorCode { get; set; }
    /// <summary>
    /// Gets or sets the error details.
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisException"/> class.
    /// </summary>
    public AnalysisException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisException"/> class.
    /// </summary>
    public AnalysisException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisException"/> class.
    /// </summary>
    public AnalysisException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisException"/> class.
    /// </summary>
    public AnalysisException(string message, string errorCode, string errorDetails)
        : base(message)
    {
        ErrorCode = errorCode;
        ErrorDetails = errorDetails;
    }
}
// ... (keep the rest of the file, just add IntegrationException below)

/// <summary>
/// Exception for invalid SQL queries
/// </summary>
public class InvalidQueryException : AnalysisException
{
    /// <summary>
    /// Gets or sets the query.
    /// </summary>
    public string Query { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the line number.
    /// </summary>
    public int? LineNumber { get; set; }
    /// <summary>
    /// Gets or sets the column number.
    /// </summary>
    public int? ColumnNumber { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidQueryException"/> class.
    /// </summary>
    public InvalidQueryException(string message) : base(message, "INVALID_QUERY") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidQueryException"/> class.
    /// </summary>
    public InvalidQueryException(string message, string query)
        : base(message, "INVALID_QUERY")
    {
        Query = query;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidQueryException"/> class with the pipeline stage
    /// that rejected the query recorded in <see cref="AnalysisException.ErrorDetails"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="query">The query text that was rejected.</param>
    /// <param name="stage">The component that rejected the query, for example the middleware name.</param>
    public InvalidQueryException(string message, string query, string stage)
        : base(message, "INVALID_QUERY", stage)
    {
        Query = query;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidQueryException"/> class.
    /// </summary>
    public InvalidQueryException(string message, string query, int lineNumber, int columnNumber)
        : base(message, "INVALID_QUERY")
    {
        Query = query;
        LineNumber = lineNumber;
        ColumnNumber = columnNumber;
    }
}

/// <summary>
/// Exception for database connection errors
/// </summary>
public class DatabaseConnectionException : AnalysisException
{
    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    public string? ConnectionString { get; set; }
    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseConnectionException"/> class.
    /// </summary>
    public DatabaseConnectionException(string message)
        : base(message, "DB_CONNECTION_ERROR") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseConnectionException"/> class.
    /// </summary>
    public DatabaseConnectionException(string message, Exception innerException)
        : base(message, "DB_CONNECTION_ERROR", innerException.Message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseConnectionException"/> class.
    /// </summary>
    public DatabaseConnectionException(string message, string databaseName, Exception innerException)
        : base(message, "DB_CONNECTION_ERROR", innerException.Message)
    {
        DatabaseName = databaseName;
    }
}

/// <summary>
/// Exception for query execution plan errors
/// </summary>
public class QueryPlanException : AnalysisException
{
    /// <summary>
    /// Gets or sets the plan source.
    /// </summary>
    public string? PlanSource { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryPlanException"/> class.
    /// </summary>
    public QueryPlanException(string message)
        : base(message, "PLAN_ERROR") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryPlanException"/> class.
    /// </summary>
    public QueryPlanException(string message, Exception innerException)
        : base(message, "PLAN_ERROR", innerException.Message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryPlanException"/> class.
    /// </summary>
    public QueryPlanException(string message, string planSource, Exception innerException)
        : base(message, "PLAN_ERROR", innerException.Message)
    {
        PlanSource = planSource;
    }
}

/// <summary>
/// Exception for index analysis errors
/// </summary>
public class IndexAnalysisException : AnalysisException
{
    /// <summary>
    /// Gets or sets the index name.
    /// </summary>
    public string? IndexName { get; set; }
    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexAnalysisException"/> class.
    /// </summary>
    public IndexAnalysisException(string message)
        : base(message, "INDEX_ANALYSIS_ERROR") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexAnalysisException"/> class.
    /// </summary>
    public IndexAnalysisException(string message, string indexName)
        : base(message, "INDEX_ANALYSIS_ERROR")
    {
        IndexName = indexName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexAnalysisException"/> class.
    /// </summary>
    public IndexAnalysisException(string message, string indexName, string tableName)
        : base(message, "INDEX_ANALYSIS_ERROR")
    {
        IndexName = indexName;
        TableName = tableName;
    }
}

/// <summary>
/// Exception for configuration errors
/// </summary>
public class ConfigurationException : AnalysisException
{
    /// <summary>
    /// Gets or sets the config key.
    /// </summary>
    public string? ConfigKey { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
    /// </summary>
    public ConfigurationException(string message)
        : base(message, "CONFIGURATION_ERROR") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
    /// </summary>
    public ConfigurationException(string message, string configKey)
        : base(message, "CONFIGURATION_ERROR")
    {
        ConfigKey = configKey;
    }
}

/// <summary>
/// Exception for repository operations
/// </summary>
public class RepositoryException : AnalysisException
{
    /// <summary>
    /// Gets or sets the operation type.
    /// </summary>
    public string? OperationType { get; set; }
    /// <summary>
    /// Gets or sets the resource ID.
    /// </summary>
    public string? ResourceId { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryException"/> class.
    /// </summary>
    public RepositoryException(string message)
        : base(message, "REPOSITORY_ERROR") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryException"/> class.
    /// </summary>
    public RepositoryException(string message, string operationType)
        : base(message, "REPOSITORY_ERROR")
    {
        OperationType = operationType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryException"/> class.
    /// </summary>
    public RepositoryException(string message, string operationType, Exception innerException)
        : base(message, "REPOSITORY_ERROR", innerException.Message)
    {
        OperationType = operationType;
    }
}

/// <summary>
/// Exception for validation errors
/// </summary>
public class ValidationException : AnalysisException
{
    /// <summary>
    /// Gets or sets the validation field.
    /// </summary>
    public string? ValidationField { get; set; }
    /// <summary>
    /// Gets or sets the validation rule.
    /// </summary>
    public string? ValidationRule { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    public ValidationException(string message)
        : base(message, "VALIDATION_ERROR") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    public ValidationException(string message, string field)
        : base(message, "VALIDATION_ERROR")
    {
        ValidationField = field;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    public ValidationException(string message, string field, string rule)
        : base(message, "VALIDATION_ERROR")
    {
        ValidationField = field;
        ValidationRule = rule;
    }
}

/// <summary>
/// Exception for integration errors
/// </summary>
public class IntegrationException : AnalysisException
{
    /// <summary>
    /// Gets or sets the service name.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationException"/> class.
    /// </summary>
    public IntegrationException(string message)
        : base(message, "INTEGRATION_ERROR") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationException"/> class.
    /// </summary>
    public IntegrationException(string message, string serviceName)
        : base(message, "INTEGRATION_ERROR")
    {
        ServiceName = serviceName;
    }
}
