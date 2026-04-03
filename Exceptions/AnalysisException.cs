// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SqlQueryAnalyzer.Exceptions;

/// <summary>
/// Base exception for all analysis-related errors
/// </summary>
public class AnalysisException : Exception
{
    public string? ErrorCode { get; set; }
    public string? ErrorDetails { get; set; }

    public AnalysisException(string message) : base(message) { }

    public AnalysisException(string message, Exception innerException)
        : base(message, innerException) { }

    public AnalysisException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public AnalysisException(string message, string errorCode, string errorDetails)
        : base(message)
    {
        ErrorCode = errorCode;
        ErrorDetails = errorDetails;
    }
}

/// <summary>
/// Exception for invalid SQL queries
/// </summary>
public class InvalidQueryException : AnalysisException
{
    public string Query { get; set; } = string.Empty;
    public int? LineNumber { get; set; }
    public int? ColumnNumber { get; set; }

    public InvalidQueryException(string message) : base(message, "INVALID_QUERY") { }

    public InvalidQueryException(string message, string query)
        : base(message, "INVALID_QUERY")
    {
        Query = query;
    }

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
    public string? ConnectionString { get; set; }
    public string? DatabaseName { get; set; }

    public DatabaseConnectionException(string message)
        : base(message, "DB_CONNECTION_ERROR") { }

    public DatabaseConnectionException(string message, Exception innerException)
        : base(message, "DB_CONNECTION_ERROR", innerException.Message) { }

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
    public string? PlanSource { get; set; }

    public QueryPlanException(string message)
        : base(message, "PLAN_ERROR") { }

    public QueryPlanException(string message, Exception innerException)
        : base(message, "PLAN_ERROR", innerException.Message) { }

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
    public string? IndexName { get; set; }
    public string? TableName { get; set; }

    public IndexAnalysisException(string message)
        : base(message, "INDEX_ANALYSIS_ERROR") { }

    public IndexAnalysisException(string message, string indexName)
        : base(message, "INDEX_ANALYSIS_ERROR")
    {
        IndexName = indexName;
    }

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
    public string? ConfigKey { get; set; }

    public ConfigurationException(string message)
        : base(message, "CONFIGURATION_ERROR") { }

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
    public string? OperationType { get; set; }
    public string? ResourceId { get; set; }

    public RepositoryException(string message)
        : base(message, "REPOSITORY_ERROR") { }

    public RepositoryException(string message, string operationType)
        : base(message, "REPOSITORY_ERROR")
    {
        OperationType = operationType;
    }

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
    public string? ValidationField { get; set; }
    public string? ValidationRule { get; set; }

    public ValidationException(string message)
        : base(message, "VALIDATION_ERROR") { }

    public ValidationException(string message, string field)
        : base(message, "VALIDATION_ERROR")
    {
        ValidationField = field;
    }

    public ValidationException(string message, string field, string rule)
        : base(message, "VALIDATION_ERROR")
    {
        ValidationField = field;
        ValidationRule = rule;
    }
}
