#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SqlQueryAnalyzer.Constants;

/// <summary>
/// Default configuration values for the analyzer
/// </summary>
public static class AnalyzerDefaults
{
    // Performance thresholds
    /// <summary>
    /// The high execution time threshold ms.
    /// </summary>
    public const double HighExecutionTimeThresholdMs = 1000.0;
    /// <summary>
    /// The moderate execution time threshold ms.
    /// </summary>
    public const double ModerateExecutionTimeThresholdMs = 500.0;
    /// <summary>
    /// The high logical reads threshold.
    /// </summary>
    public const double HighLogicalReadsThreshold = 100000;
    /// <summary>
    /// The moderate logical reads threshold.
    /// </summary>
    public const double ModerateLogicalReadsThreshold = 10000;
    /// <summary>
    /// The high physical reads threshold.
    /// </summary>
    public const double HighPhysicalReadsThreshold = 10000;

    // Index fragmentation thresholds
    /// <summary>
    /// The fragmentation rebuild threshold.
    /// </summary>
    public const double FragmentationRebuildThreshold = 30.0;
    /// <summary>
    /// The fragmentation reorganize threshold.
    /// </summary>
    public const double FragmentationReorganizeThreshold = 10.0;
    /// <summary>
    /// The optimal fragmentation threshold.
    /// </summary>
    public const double OptimalFragmentationThreshold = 5.0;

    // Query complexity thresholds
    /// <summary>
    /// The simple complexity line count.
    /// </summary>
    public const int SimpleComplexityLineCount = 5;
    /// <summary>
    /// The low complexity line count.
    /// </summary>
    public const int LowComplexityLineCount = 10;
    /// <summary>
    /// The medium complexity line count.
    /// </summary>
    public const int MediumComplexityLineCount = 20;
    /// <summary>
    /// The high complexity line count.
    /// </summary>
    public const int HighComplexityLineCount = 50;

    /// <summary>
    /// The simple complexity table count.
    /// </summary>
    public const int SimpleComplexityTableCount = 1;
    /// <summary>
    /// The low complexity table count.
    /// </summary>
    public const int LowComplexityTableCount = 2;
    /// <summary>
    /// The medium complexity table count.
    /// </summary>
    public const int MediumComplexityTableCount = 3;
    /// <summary>
    /// The high complexity table count.
    /// </summary>
    public const int HighComplexityTableCount = 5;

    // Issue severity scoring
    /// <summary>
    /// The critical issue penalty.
    /// </summary>
    public const double CriticalIssuePenalty = 10.0;
    /// <summary>
    /// The warning issue penalty.
    /// </summary>
    public const double WarningIssuePenalty = 5.0;
    /// <summary>
    /// The info issue penalty.
    /// </summary>
    public const double InfoIssuePenalty = 2.0;
    /// <summary>
    /// The optimization potential bonus.
    /// </summary>
    public const double OptimizationPotentialBonus = 0.1;

    // Index recommendation thresholds
    /// <summary>
    /// The min index performance gain.
    /// </summary>
    public const double MinIndexPerformanceGain = 5.0;
    /// <summary>
    /// The min index affected queries.
    /// </summary>
    public const int MinIndexAffectedQueries = 1;
    /// <summary>
    /// The max index maintenance cost ratio.
    /// </summary>
    public const double MaxIndexMaintenanceCostRatio = 0.3;

    // Statistics update frequency
    /// <summary>
    /// The statistics staleness days.
    /// </summary>
    public const int StatisticsStalenessDays = 7;
    /// <summary>
    /// The statistics check interval days.
    /// </summary>
    public const int StatisticsCheckIntervalDays = 1;

    // Analysis timeouts
    /// <summary>
    /// The default analysis timeout seconds.
    /// </summary>
    public const int DefaultAnalysisTimeoutSeconds = 60;
    /// <summary>
    /// The batch analysis timeout seconds.
    /// </summary>
    public const int BatchAnalysisTimeoutSeconds = 300;
    /// <summary>
    /// The index analysis timeout seconds.
    /// </summary>
    public const int IndexAnalysisTimeoutSeconds = 30;

    // Performance score calculation weights
    /// <summary>
    /// The issue weight critical.
    /// </summary>
    public const double IssueWeightCritical = 1.0;
    /// <summary>
    /// The issue weight warning.
    /// </summary>
    public const double IssueWeightWarning = 0.5;
    /// <summary>
    /// The issue weight info.
    /// </summary>
    public const double IssueWeightInfo = 0.1;
    /// <summary>
    /// The optimization weight.
    /// </summary>
    public const double OptimizationWeight = 0.8;

    // Report generation
    /// <summary>
    /// The max issues in report.
    /// </summary>
    public const int MaxIssuesInReport = 50;
    /// <summary>
    /// The max suggestions in report.
    /// </summary>
    public const int MaxSuggestionsInReport = 10;
    /// <summary>
    /// The report summary line length.
    /// </summary>
    public const int ReportSummaryLineLength = 80;

    // Database compatibility
    /// <summary>
    /// The SQL server version.
    /// </summary>
    public const string SqlServerVersion = "2016+";
    /// <summary>
    /// The postgre SQL version.
    /// </summary>
    public const string PostgreSqlVersion = "12+";
    /// <summary>
    /// The my SQL version.
    /// </summary>
    public const string MySqlVersion = "5.7+";

    // Cache settings
    /// <summary>
    /// The analysis cache expiration minutes.
    /// </summary>
    public const int AnalysisCacheExpirationMinutes = 60;
    /// <summary>
    /// The max cached analyses.
    /// </summary>
    public const int MaxCachedAnalyses = 10000;

    // Batch processing
    /// <summary>
    /// The max batch size.
    /// </summary>
    public const int MaxBatchSize = 100;
    /// <summary>
    /// The default batch size.
    /// </summary>
    public const int DefaultBatchSize = 10;
    /// <summary>
    /// The max concurrent analyses.
    /// </summary>
    public const int MaxConcurrentAnalyses = 4;

    // Resource limits
    /// <summary>
    /// The max query size bytes.
    /// </summary>
    public const long MaxQuerySizeBytes = 1048576; // 1 MB
    /// <summary>
    /// The max query statements.
    /// </summary>
    public const int MaxQueryStatements = 1000;
    /// <summary>
    /// The max query parameters.
    /// </summary>
    public const int MaxQueryParameters = 500;

    // Default scoring ranges
    /// <summary>
    /// The scoring ranges.
    /// </summary>
    public static class ScoringRanges
    {
        /// <summary>
        /// The excellent.
        /// </summary>
        public const double Excellent = 90.0;
        /// <summary>
        /// The good.
        /// </summary>
        public const double Good = 75.0;
        /// <summary>
        /// The acceptable.
        /// </summary>
        public const double Acceptable = 60.0;
        /// <summary>
        /// The poor.
        /// </summary>
        public const double Poor = 40.0;
        /// <summary>
        /// The critical.
        /// </summary>
        public const double Critical = 20.0;
    }

    // Detection sensitivity levels
    /// <summary>
    /// The detection sensitivity.
    /// </summary>
    public enum DetectionSensitivity
    {
        /// <summary>
        /// The low.
        /// </summary>
        Low = 1,
        /// <summary>
        /// The medium.
        /// </summary>
        Medium = 2,
        /// <summary>
        /// The high.
        /// </summary>
        High = 3,
        /// <summary>
        /// The very high.
        /// </summary>
        VeryHigh = 4,
    }

    // Default detection sensitivity
    /// <summary>
    /// The default sensitivity.
    /// </summary>
    public const DetectionSensitivity DefaultSensitivity = DetectionSensitivity.High;

    // Error codes
    /// <summary>
    /// The error codes.
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// The invalid query.
        /// </summary>
        public const string InvalidQuery = "ERR_INVALID_QUERY";
        /// <summary>
        /// The analysis failed.
        /// </summary>
        public const string AnalysisFailed = "ERR_ANALYSIS_FAILED";
        /// <summary>
        /// The connection error.
        /// </summary>
        public const string ConnectionError = "ERR_CONNECTION";
        /// <summary>
        /// The timeout.
        /// </summary>
        public const string Timeout = "ERR_TIMEOUT";
        /// <summary>
        /// The config error.
        /// </summary>
        public const string ConfigError = "ERR_CONFIG";
        /// <summary>
        /// The database error.
        /// </summary>
        public const string DatabaseError = "ERR_DATABASE";
    }

    // Success codes
    /// <summary>
    /// The success codes.
    /// </summary>
    public static class SuccessCodes
    {
        /// <summary>
        /// The analysis complete.
        /// </summary>
        public const string AnalysisComplete = "OK_ANALYSIS_COMPLETE";
        /// <summary>
        /// The no issues found.
        /// </summary>
        public const string NoIssuesFound = "OK_NO_ISSUES";
        /// <summary>
        /// The suggestions provided.
        /// </summary>
        public const string SuggestionsProvided = "OK_SUGGESTIONS";
    }
}
