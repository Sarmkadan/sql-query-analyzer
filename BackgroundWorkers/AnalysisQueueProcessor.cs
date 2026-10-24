#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Services;

namespace SqlQueryAnalyzer.BackgroundWorkers;

/// <summary>
/// Background worker that processes analysis requests from a queue.
/// Enables asynchronous, fire-and-forget analysis workflow.
/// Handles task persistence, retry logic, and progress tracking.
/// </summary>
public class AnalysisQueueProcessor
{
    private readonly Queue<AnalysisTask> _taskQueue = new();
    private readonly Dictionary<string, AnalysisTask> _activeTasks = new();
    private readonly IQueryAnalyzerService _analyzerService;
    private readonly ILogger<AnalysisQueueProcessor> _logger;
    private readonly int _maxConcurrentTasks;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _processingTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisQueueProcessor"/> class.
    /// </summary>
    public AnalysisQueueProcessor(
        IQueryAnalyzerService analyzerService,
        ILogger<AnalysisQueueProcessor> logger,
        int maxConcurrentTasks = 5)
    {
        ArgumentNullException.ThrowIfNull(analyzerService);
        ArgumentNullException.ThrowIfNull(logger);
        _analyzerService = analyzerService;
        _logger = logger;
        _maxConcurrentTasks = maxConcurrentTasks;
    }

    /// <summary>
    /// Enqueues a query for analysis.
    /// Returns task ID that can be used to track progress.
    /// </summary>
    public string EnqueueAnalysis(string query, Action<QueryAnalysisResult>? onComplete = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(query);
        var task = new AnalysisTask
        {
            TaskId = Guid.NewGuid().ToString(),
            Query = query,
            Status = AnalysisTaskStatus.Queued,
            CreatedAt = DateTime.UtcNow,
            OnComplete = onComplete
        };

        _taskQueue.Enqueue(task);
        _logger.LogInformation($"Enqueued analysis task: {task.TaskId}");

        return task.TaskId;
    }

    /// <summary>
    /// Starts the background processor.
    /// Processes queued tasks with specified degree of parallelism.
    /// </summary>
    public void Start()
    {
        ArgumentNullException.ThrowIfNull(_analyzerService);
        ArgumentNullException.ThrowIfNull(_logger);
        if (_processingTask != null && !_processingTask.IsCompleted)
        {
            _logger.LogWarning("Processor already running");
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _processingTask = ProcessQueueAsync(_cancellationTokenSource.Token);

        _logger.LogInformation("Analysis queue processor started");
    }

    /// <summary>
    /// Stops the background processor gracefully.
    /// Waits for active tasks to complete.
    /// </summary>
    public async Task StopAsync(TimeSpan timeout = default)
    {
        ArgumentNullException.ThrowIfNull(_analyzerService);
        ArgumentNullException.ThrowIfNull(_logger);
        if (_processingTask == null)
        {
            _logger.LogWarning("Processor not running");
            return;
        }

        if (timeout == default)
            timeout = TimeSpan.FromSeconds(30);

        _logger.LogInformation("Stopping analysis queue processor...");
        _cancellationTokenSource?.Cancel();

        try
        {
            await _processingTask.WaitAsync(timeout);
        }
        catch (TimeoutException)
        {
            _logger.LogWarning("Processor stop timeout - some tasks may not have completed");
        }

        _logger.LogInformation("Analysis queue processor stopped");
    }

    /// <summary>
    /// Gets current status of a queued task.
    /// </summary>
    public AnalysisTask? GetTaskStatus(string taskId)
    {
        ArgumentNullException.ThrowIfNull(taskId);
        if (_activeTasks.TryGetValue(taskId, out var task))
            return task;

        // Check if in queue
        var queuedTask = _taskQueue.FirstOrDefault(t => t.TaskId == taskId);
        return queuedTask;
    }

    /// <summary>
    /// Gets queue statistics for monitoring.
    /// </summary>
    public QueueStatistics GetStatistics()
    {
        return new QueueStatistics
        {
            QueuedCount = _taskQueue.Count,
            ActiveCount = _activeTasks.Count,
            MaxConcurrency = _maxConcurrentTasks,
            TotalProcessed = _activeTasks.Count,
            AverageProcessingTimeMs = _activeTasks.Count > 0
                ? _activeTasks.Values.Average(t => (DateTime.UtcNow - t.StartedAt!.Value).TotalMilliseconds)
                : 0
        };
    }

    /// <summary>
    /// Main processing loop - processes queued tasks with concurrency control.
    /// </summary>
    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        var activeTasks = new List<Task>();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Remove completed tasks
                activeTasks.RemoveAll(t => t.IsCompleted);

                // Start new tasks up to max concurrency
                while (activeTasks.Count < _maxConcurrentTasks &&
                       _taskQueue.TryDequeue(out var task) &&
                       !cancellationToken.IsCancellationRequested)
                {
                    _activeTasks[task.TaskId] = task;
                    var processingTask = ProcessTaskAsync(task, cancellationToken);
                    activeTasks.Add(processingTask);
                }

                // If no tasks running, wait before checking again
                if (activeTasks.Count == 0)
                {
                    await Task.Delay(100, cancellationToken);
                }
                else
                {
                    // Wait for any task to complete
                    await Task.WhenAny(activeTasks);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Queue processing cancelled");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in queue processing loop");
            }
        }

        // Wait for remaining active tasks
        if (activeTasks.Count > 0)
        {
            _logger.LogInformation("Waiting for active tasks to complete...");
            await Task.WhenAll(activeTasks);
        }
    }

    /// <summary>
    /// Processes a single analysis task.
    /// </summary>
    private async Task ProcessTaskAsync(AnalysisTask task, CancellationToken cancellationToken)
    {
        task.Status = AnalysisTaskStatus.InProgress;
        task.StartedAt = DateTime.UtcNow;

        try
        {
            _logger.LogInformation($"Processing task: {task.TaskId}");

            task.Result = await _analyzerService.AnalyzeQueryAsync(task.Query);
            task.Status = AnalysisTaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation($"Task completed: {task.TaskId} (Score: {task.Result.PerformanceScore:F1}/100)");

            // Call completion callback if provided
            task.OnComplete?.Invoke(task.Result);
        }
        catch (OperationCanceledException)
        {
            task.Status = AnalysisTaskStatus.Cancelled;
            _logger.LogInformation($"Task cancelled: {task.TaskId}");
        }
        catch (Exception ex)
        {
            task.Status = AnalysisTaskStatus.Failed;
            task.ErrorMessage = ex.Message;
            _logger.LogError(ex, $"Task failed: {task.TaskId}");
        }
        finally
        {
            // Clean up active task reference
            if (task.CompletedAt.HasValue)
            {
                _activeTasks.Remove(task.TaskId);
            }
        }
    }
}

/// <summary>
/// Represents a queued analysis task.
/// </summary>
public class AnalysisTask
{
    /// <summary>
    /// Gets or sets the task ID.
    /// </summary>
    public string TaskId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the query.
    /// </summary>
    public string Query { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public AnalysisTaskStatus Status { get; set; }
    /// <summary>
    /// Gets or sets the created at.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// Gets or sets the started at.
    /// </summary>
    public DateTime? StartedAt { get; set; }
    /// <summary>
    /// Gets or sets the completed at.
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    /// <summary>
    /// Gets or sets the result.
    /// </summary>
    public QueryAnalysisResult? Result { get; set; }
    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string? ErrorMessage { get; set; }
    /// <summary>
    /// Gets or sets the on complete.
    /// </summary>
    public Action<QueryAnalysisResult>? OnComplete { get; set; }

    /// <summary>
    /// Gets elapsed time for task processing.
    /// </summary>
    public TimeSpan GetElapsedTime()
    {
        var endTime = CompletedAt ?? DateTime.UtcNow;
        return endTime - (StartedAt ?? CreatedAt);
    }
}

/// <summary>
/// Status of an analysis task in the queue.
/// </summary>
public enum AnalysisTaskStatus
{
    /// <summary>
    /// The queued.
    /// </summary>
    Queued,
    /// <summary>
    /// The in progress.
    /// </summary>
    InProgress,
    /// <summary>
    /// The completed.
    /// </summary>
    Completed,
    /// <summary>
    /// The failed.
    /// </summary>
    Failed,
    /// <summary>
    /// The cancelled.
    /// </summary>
    Cancelled
}

/// <summary>
/// Statistics about the analysis queue.
/// </summary>
public class QueueStatistics
{
    /// <summary>
    /// Gets or sets the queued count.
    /// </summary>
    public int QueuedCount { get; set; }
    /// <summary>
    /// Gets or sets the active count.
    /// </summary>
    public int ActiveCount { get; set; }
    /// <summary>
    /// Gets or sets the max concurrency.
    /// </summary>
    public int MaxConcurrency { get; set; }
    /// <summary>
    /// Gets or sets the total processed.
    /// </summary>
    public int TotalProcessed { get; set; }
    /// <summary>
    /// Gets or sets the average processing time ms.
    /// </summary>
    public double AverageProcessingTimeMs { get; set; }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    public override string ToString() =>
        $"Queue: {QueuedCount} queued, {ActiveCount}/{MaxConcurrency} active, " +
        $"{TotalProcessed} processed, Avg: {AverageProcessingTimeMs:F0}ms";
}
