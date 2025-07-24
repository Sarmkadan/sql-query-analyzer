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
    public const double HighExecutionTimeThresholdMs = 1000.0;
    public const double ModerateExecutionTimeThresholdMs = 500.0;
    public const double HighLogicalReadsThreshold = 100000;
    public const double ModerateLogicalReadsThreshold = 10000;
    public const double HighPhysicalReadsThreshold = 10000;

    // Index fragmentation thresholds
    public const double FragmentationRebuildThreshold = 30.0;
    public const double FragmentationReorganizeThreshold = 10.0;
    public const double OptimalFragmentationThreshold = 5.0;

    // Query complexity thresholds
    public const int SimpleComplexityLineCount = 5;
    public const int LowComplexityLineCount = 10;
    public const int MediumComplexityLineCount = 20;
    public const int HighComplexityLineCount = 50;

    public const int SimpleComplexityTableCount = 1;
    public const int LowComplexityTableCount = 2;
    public const int MediumComplexityTableCount = 3;
    public const int HighComplexityTableCount = 5;

    // Issue severity scoring
    public const double CriticalIssuePenalty = 10.0;
    public const double WarningIssuePenalty = 5.0;
    public const double InfoIssuePenalty = 2.0;
    public const double OptimizationPotentialBonus = 0.1;

    // Index recommendation thresholds
    public const double MinIndexPerformanceGain = 5.0;
    public const int MinIndexAffectedQueries = 1;
    public const double MaxIndexMaintenanceCostRatio = 0.3;

    // Statistics update frequency
    public const int StatisticsStalenessDays = 7;
    public const int StatisticsCheckIntervalDays = 1;

    // Analysis timeouts
    public const int DefaultAnalysisTimeoutSeconds = 60;
    public const int BatchAnalysisTimeoutSeconds = 300;
    public const int IndexAnalysisTimeoutSeconds = 30;

    // Performance score calculation weights
    public const double IssueWeightCritical = 1.0;
    public const double IssueWeightWarning = 0.5;
    public const double IssueWeightInfo = 0.1;
    public const double OptimizationWeight = 0.8;

    // Report generation
    public const int MaxIssuesInReport = 50;
    public const int MaxSuggestionsInReport = 10;
    public const int ReportSummaryLineLength = 80;

    // Database compatibility
    public const string SqlServerVersion = "2016+";
    public const string PostgreSqlVersion = "12+";
    public const string MySqlVersion = "5.7+";

    // Cache settings
    public const int AnalysisCacheExpirationMinutes = 60;
    public const int MaxCachedAnalyses = 10000;

    // Batch processing
    public const int MaxBatchSize = 100;
    public const int DefaultBatchSize = 10;
    public const int MaxConcurrentAnalyses = 4;

    // Resource limits
    public const long MaxQuerySizeBytes = 1048576; // 1 MB
    public const int MaxQueryStatements = 1000;
    public const int MaxQueryParameters = 500;

    // Default scoring ranges
    public static class ScoringRanges
    {
        public const double Excellent = 90.0;
        public const double Good = 75.0;
        public const double Acceptable = 60.0;
        public const double Poor = 40.0;
        public const double Critical = 20.0;
    }

    // Detection sensitivity levels
    public enum DetectionSensitivity
    {
        Low = 1,
        Medium = 2,
        High = 3,
        VeryHigh = 4
    }

    // Default detection sensitivity
    public const DetectionSensitivity DefaultSensitivity = DetectionSensitivity.High;

    // Error codes
    public static class ErrorCodes
    {
        public const string InvalidQuery = "ERR_INVALID_QUERY";
        public const string AnalysisFailed = "ERR_ANALYSIS_FAILED";
        public const string ConnectionError = "ERR_CONNECTION";
        public const string Timeout = "ERR_TIMEOUT";
        public const string ConfigError = "ERR_CONFIG";
        public const string DatabaseError = "ERR_DATABASE";
    }

    // Success codes
    public static class SuccessCodes
    {
        public const string AnalysisComplete = "OK_ANALYSIS_COMPLETE";
        public const string NoIssuesFound = "OK_NO_ISSUES";
        public const string SuggestionsProvided = "OK_SUGGESTIONS";
    }
}
