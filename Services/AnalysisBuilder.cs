#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using SqlQueryAnalyzer.DTOs;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Builder class for creating analysis requests and configuring analysis options
/// </summary>
public sealed class AnalysisBuilder
{
    private AnalysisRequestDto _request = new();
    private readonly List<string> _validationErrors = [];

    // Set query text
    /// <summary>
    /// With query.
    /// </summary>
    public AnalysisBuilder WithQuery(string queryText)
    {
        ArgumentException.ThrowIfNullOrEmpty(queryText);
        if (string.IsNullOrWhiteSpace(queryText))
        {
            _validationErrors.Add("Query text cannot be empty");
        }
        else if (queryText.Length > 1048576) // 1MB
        {
            _validationErrors.Add("Query text exceeds maximum size of 1MB");
        }
        else
        {
            _request.QueryText = queryText;
        }

        return this;
    }

    // Set application context
    /// <summary>
    /// With application.
    /// </summary>
    public AnalysisBuilder WithApplication(string applicationName)
    {
        ArgumentException.ThrowIfNullOrEmpty(applicationName);
        if (!string.IsNullOrWhiteSpace(applicationName))
            _request.ApplicationName = applicationName;
        return this;
    }

    // Set procedure context
    /// <summary>
    /// With procedure.
    /// </summary>
    public AnalysisBuilder WithProcedure(string procedureName)
    {
        ArgumentException.ThrowIfNullOrEmpty(procedureName);
        if (!string.IsNullOrWhiteSpace(procedureName))
            _request.ProcedureName = procedureName;
        return this;
    }

    // Set module context
    /// <summary>
    /// With module.
    /// </summary>
    public AnalysisBuilder WithModule(string moduleName)
    {
        ArgumentException.ThrowIfNullOrEmpty(moduleName);
        if (!string.IsNullOrWhiteSpace(moduleName))
            _request.ModuleName = moduleName;
        return this;
    }

    // Enable/disable index suggestions
    /// <summary>
    /// Include index suggestions.
    /// </summary>
    public AnalysisBuilder IncludeIndexSuggestions(bool include = true)
    {
        _request.IncludeIndexSuggestions = include;
        return this;
    }

    // Enable/disable fragmentation analysis
    /// <summary>
    /// Analyze fragmentation.
    /// </summary>
    public AnalysisBuilder AnalyzeFragmentation(bool analyze = true)
    {
        _request.AnalyzeFragmentation = analyze;
        return this;
    }

    // Enable/disable execution plan analysis
    /// <summary>
    /// Analyze plan.
    /// </summary>
    public AnalysisBuilder AnalyzePlan(bool analyze = true)
    {
        _request.AnalyzePlan = analyze;
        return this;
    }

    // Set execution plan XML
    /// <summary>
    /// With execution plan.
    /// </summary>
    public AnalysisBuilder WithExecutionPlan(string planXml)
    {
        ArgumentException.ThrowIfNullOrEmpty(planXml);
        if (!string.IsNullOrWhiteSpace(planXml))
            _request.ExecutionPlanXml = planXml;
        return this;
    }

    // Validate and build
    /// <summary>
    /// Build.
    /// </summary>
    public AnalysisRequestDto Build()
    {
        if (string.IsNullOrWhiteSpace(_request.QueryText))
            throw new InvalidOperationException("Query text is required");

        if (!QueryValidator.IsValidQuery(_request.QueryText))
            throw new InvalidOperationException("Query text is not valid SQL");

        if (_validationErrors.Count > 0)
            throw new InvalidOperationException($"Validation errors: {string.Join(", ", _validationErrors)}");

        return _request;
    }

    // Reset builder
    /// <summary>
    /// Reset.
    /// </summary>
    public AnalysisBuilder Reset()
    {
        _request = new();
        _validationErrors.Clear();
        return this;
    }

    // Fluent convenience methods
    /// <summary>
    /// Full.
    /// </summary>
    public AnalysisBuilder Full()
    {
        _request.IncludeIndexSuggestions = true;
        _request.AnalyzeFragmentation = true;
        _request.AnalyzePlan = true;
        return this;
    }

    /// <summary>
    /// Quick.
    /// </summary>
    public AnalysisBuilder Quick()
    {
        _request.IncludeIndexSuggestions = false;
        _request.AnalyzeFragmentation = false;
        _request.AnalyzePlan = false;
        return this;
    }

    // Get validation errors
    /// <summary>
    /// Get errors.
    /// </summary>
    public List<string> GetErrors() => new(_validationErrors);

    // Check if builder is valid
    /// <summary>
    /// Is valid.
    /// </summary>
    public bool IsValid() => _validationErrors.Count == 0 && !string.IsNullOrWhiteSpace(_request.QueryText);
}

/// <summary>
/// Builder for batch analysis requests
/// </summary>
public sealed class BatchAnalysisBuilder
{
    private BatchAnalysisRequestDto _request = new();
    private readonly List<string> _validationErrors = [];

    // Add single query
    /// <summary>
    /// Add query.
    /// </summary>
    public BatchAnalysisBuilder AddQuery(string queryText)
    {
        if (!string.IsNullOrWhiteSpace(queryText))
            _request.Queries.Add(queryText);
        return this;
    }

    // Add multiple queries
    /// <summary>
    /// Add queries.
    /// </summary>
    public BatchAnalysisBuilder AddQueries(params string[] queries)
    {
        foreach (var query in queries)
        {
            if (!string.IsNullOrWhiteSpace(query))
                _request.Queries.Add(query);
        }
        return this;
    }

    // Add queries from collection
    /// <summary>
    /// Add queries.
    /// </summary>
    public BatchAnalysisBuilder AddQueries(IEnumerable<string> queries)
    {
        foreach (var query in queries)
        {
            if (!string.IsNullOrWhiteSpace(query))
                _request.Queries.Add(query);
        }
        return this;
    }

    // Set application context
    /// <summary>
    /// With application.
    /// </summary>
    public BatchAnalysisBuilder WithApplication(string applicationName)
    {
        if (!string.IsNullOrWhiteSpace(applicationName))
            _request.ApplicationName = applicationName;
        return this;
    }

    // Enable pattern analysis
    /// <summary>
    /// Analyze patterns.
    /// </summary>
    public BatchAnalysisBuilder AnalyzePatterns(bool analyze = true)
    {
        _request.AnalyzePatterns = analyze;
        return this;
    }

    // Set timeout
    /// <summary>
    /// With timeout.
    /// </summary>
    public BatchAnalysisBuilder WithTimeout(int seconds)
    {
        if (seconds > 0 && seconds <= 3600)
            _request.TimeoutSeconds = seconds;
        else
            _validationErrors.Add("Timeout must be between 1 and 3600 seconds");
        return this;
    }

    // Build with validation
    /// <summary>
    /// Build.
    /// </summary>
    public BatchAnalysisRequestDto Build()
    {
        if (_request.Queries.Count == 0)
            throw new InvalidOperationException("At least one query is required");

        if (_request.Queries.Count > 100)
            throw new InvalidOperationException("Maximum 100 queries per batch");

        if (_validationErrors.Count > 0)
            throw new InvalidOperationException($"Validation errors: {string.Join(", ", _validationErrors)}");

        return _request;
    }

    // Reset builder
    /// <summary>
    /// Reset.
    /// </summary>
    public BatchAnalysisBuilder Reset()
    {
        _request = new();
        _validationErrors.Clear();
        return this;
    }

    // Get validation errors
    /// <summary>
    /// Get errors.
    /// </summary>
    public List<string> GetErrors() => new(_validationErrors);

    // Check validity
    /// <summary>
    /// Is valid.
    /// </summary>
    public bool IsValid() => _validationErrors.Count == 0 && _request.Queries.Count > 0;
}

/// <summary>
/// Builder for index analysis requests
/// </summary>
public sealed class IndexAnalysisBuilder
{
    private IndexAnalysisRequestDto _request = new();
    private readonly List<string> _validationErrors = [];

    // Set table name
    /// <summary>
    /// For table.
    /// </summary>
    public IndexAnalysisBuilder ForTable(string tableName)
    {
        if (!string.IsNullOrWhiteSpace(tableName))
            _request.TableName = tableName;
        else
            _validationErrors.Add("Table name is required");
        return this;
    }

    // Include fragmentation analysis
    /// <summary>
    /// Include fragmentation.
    /// </summary>
    public IndexAnalysisBuilder IncludeFragmentation(bool include = true)
    {
        _request.IncludeFragmentation = include;
        return this;
    }

    // Include unused index detection
    /// <summary>
    /// Include unused.
    /// </summary>
    public IndexAnalysisBuilder IncludeUnused(bool include = true)
    {
        _request.IncludeUnused = include;
        return this;
    }

    // Include script generation
    /// <summary>
    /// Generate scripts.
    /// </summary>
    public IndexAnalysisBuilder GenerateScripts(bool generate = true)
    {
        _request.GenerateScripts = generate;
        return this;
    }

    // Full analysis
    /// <summary>
    /// Full.
    /// </summary>
    public IndexAnalysisBuilder Full()
    {
        _request.IncludeFragmentation = true;
        _request.IncludeUnused = true;
        _request.GenerateScripts = true;
        return this;
    }

    // Quick analysis
    /// <summary>
    /// Quick.
    /// </summary>
    public IndexAnalysisBuilder Quick()
    {
        _request.IncludeFragmentation = false;
        _request.IncludeUnused = false;
        _request.GenerateScripts = false;
        return this;
    }

    // Build with validation
    /// <summary>
    /// Build.
    /// </summary>
    public IndexAnalysisRequestDto Build()
    {
        if (string.IsNullOrWhiteSpace(_request.TableName))
            _validationErrors.Add("Table name is required");

        if (_validationErrors.Count > 0)
            throw new InvalidOperationException($"Validation errors: {string.Join(", ", _validationErrors)}");

        return _request;
    }

    // Reset builder
    /// <summary>
    /// Reset.
    /// </summary>
    public IndexAnalysisBuilder Reset()
    {
        _request = new();
        _validationErrors.Clear();
        return this;
    }

    // Get validation errors
    /// <summary>
    /// Get errors.
    /// </summary>
    public List<string> GetErrors() => new(_validationErrors);

    // Check validity
    /// <summary>
    /// Is valid.
    /// </summary>
    public bool IsValid() => _validationErrors.Count == 0 && !string.IsNullOrWhiteSpace(_request.TableName);
}
